using Microsoft.Extensions.Logging;
using Moq;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Gamification;
using SmartTask.Web.Services.Interfaces;
using SmartTask.Web.Tests.TestHelpers;
using Xunit;

namespace SmartTask.Web.Tests.Services;

/// <summary>
/// مزیت فعال باید واقعاً روی تجربه دریافتی هنگام تکمیل تسک اثر بگذارد
/// </summary>
public class TaskRewardCoordinatorPerkTests
{
    private static TaskRewardCoordinator CreateCoordinator(
        ApplicationDbContext context,
        Mock<INotificationService> notificationMock)
    {
        var rewardEngine = new RewardEngine(context, new Mock<ILogger<RewardEngine>>().Object);
        var achievementEngine = new AchievementEngine(
            context,
            new SmartTask.Web.Infrastructure.Events.DomainEventPublisher(),
            new Mock<ILogger<AchievementEngine>>().Object);
        var cosmetics = new EquippedCosmeticsService(
            context, new Mock<ILogger<EquippedCosmeticsService>>().Object);

        // در این تست‌ها تمرکز روی ضریب تجربه است، پس دروازه پاداش اجازه می‌دهد
        var eligibilityMock = new Mock<IRewardEligibilityService>();
        eligibilityMock
            .Setup(x => x.CanRewardTaskAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(RewardEligibility.Allow());

        return new TaskRewardCoordinator(
            context,
            rewardEngine,
            achievementEngine,
            notificationMock.Object,
            cosmetics,
            eligibilityMock.Object,
            new Mock<ILogger<TaskRewardCoordinator>>().Object);
    }

    private static void EquipPerk(ApplicationDbContext context, int userId, string perkName)
    {
        var item = new MarketplaceItem
        {
            Name = perkName,
            Description = "مزیت آزمایشی",
            Icon = "⚙️",
            Color = "#FF5500",
            Category = "Perk",
            Rarity = MarketplaceItemRarity.Uncommon,
            Price = 200,
            Stock = -1,
            IsActive = true
        };
        context.Set<MarketplaceItem>().Add(item);
        context.SaveChanges();

        context.Set<UserInventory>().Add(new UserInventory
        {
            UserId = userId,
            MarketplaceItemId = item.Id,
            Quantity = 1,
            IsEquipped = true,
            AcquiredDate = DateTime.UtcNow,
            ViewState = true
        });
        context.SaveChanges();
    }

    private static async Task<int> RunCompletionAndGetXpAsync(bool withPerk)
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var userId = context.Set<ApplicationUser>().First().Id;
        var task = context.TaskItems.First();

        var notificationMock = new Mock<INotificationService>();
        notificationMock
            .Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>()))
            .Returns(Task.CompletedTask);

        var coordinator = CreateCoordinator(context, notificationMock);
        await coordinator.EnsureUserGamificationAsync(userId);

        if (withPerk)
            EquipPerk(context, userId, "Double XP Boost");

        await coordinator.HandleTaskCompletedAsync(
            task.Id, task.Title, new[] { userId }, TaskPriorityType.Medium, 4);

        var progression = context.Set<UserProgression>().First(x => x.UserId == userId);

        // XP انباشته = تجربه باقی‌مانده + آستانه‌های مصرف‌شده در ارتقای سطح
        var consumed = 0;
        var threshold = 1000;
        for (var level = 1; level < progression.CurrentLevel; level++)
        {
            consumed += threshold;
            threshold = (int)(threshold * 1.5);
        }

        return progression.TotalExperience + consumed;
    }

    [Fact]
    public async Task HandleTaskCompletedAsync_DoubleXpPerk_GrantsMoreExperienceThanWithout()
    {
        var baseline = await RunCompletionAndGetXpAsync(withPerk: false);
        var boosted = await RunCompletionAndGetXpAsync(withPerk: true);

        Assert.True(baseline > 0, "تکمیل تسک باید تجربه بدهد");
        Assert.True(boosted > baseline,
            $"مزیت فعال باید تجربه بیشتری بدهد، اما boosted={boosted} و baseline={baseline}");
    }

    [Fact]
    public async Task HandleTaskCompletedAsync_MentionsMultiplierInNotification_WhenPerkActive()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var userId = context.Set<ApplicationUser>().First().Id;
        var task = context.TaskItems.First();

        var messages = new List<string>();
        var notificationMock = new Mock<INotificationService>();
        notificationMock
            .Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>()))
            .Callback<int, string, string, NotificationType>((_, _, message, _) => messages.Add(message))
            .Returns(Task.CompletedTask);

        var coordinator = CreateCoordinator(context, notificationMock);
        await coordinator.EnsureUserGamificationAsync(userId);
        EquipPerk(context, userId, "Double XP Boost");

        await coordinator.HandleTaskCompletedAsync(
            task.Id, task.Title, new[] { userId }, TaskPriorityType.High, 8);

        Assert.Contains(messages, m => m.Contains("مزایای فعال"));
    }
}
