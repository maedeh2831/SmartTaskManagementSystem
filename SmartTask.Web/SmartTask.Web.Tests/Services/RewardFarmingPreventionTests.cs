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
/// اکسپلویت اصلی: تکمیل تسک → تغییر وضعیت → تکمیل دوباره برای گرفتن امتیاز تکراری.
/// این تست‌ها مسیر واقعی پاداش را اجرا می‌کنند (بدون mock کردن دروازه).
/// </summary>
public class RewardFarmingPreventionTests
{
    private static TaskRewardCoordinator CreateCoordinator(ApplicationDbContext context)
    {
        var notificationMock = new Mock<INotificationService>();
        notificationMock
            .Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>()))
            .Returns(Task.CompletedTask);

        var abuseMock = new Mock<IAbuseDetectionEngine>();
        abuseMock.Setup(x => x.IsUserSuspendedAsync(It.IsAny<int>())).ReturnsAsync(false);

        return new TaskRewardCoordinator(
            context,
            new RewardEngine(context, new Mock<ILogger<RewardEngine>>().Object),
            new AchievementEngine(
                context,
                new SmartTask.Web.Infrastructure.Events.DomainEventPublisher(),
                new Mock<ILogger<AchievementEngine>>().Object),
            notificationMock.Object,
            new EquippedCosmeticsService(context, new Mock<ILogger<EquippedCosmeticsService>>().Object),
            new RewardEligibilityService(context, abuseMock.Object,
                new Mock<ILogger<RewardEligibilityService>>().Object),
            new Mock<ILogger<TaskRewardCoordinator>>().Object);
    }

    private static (int userId, TaskItem task) SeedAssignedCompletedTask(
        ApplicationDbContext context,
        TimeSpan? lifetime = null)
    {
        var userId = context.Set<ApplicationUser>().First().Id;
        var task = context.TaskItems.First();

        task.Title = "پیاده‌سازی صفحه ورود";
        task.Status = TaskStatusType.Done;
        task.CreatedDate = DateTime.Now - (lifetime ?? TimeSpan.FromHours(5));
        task.CompletedDate = DateTime.Now;

        context.Set<TaskAssignment>().Add(new TaskAssignment
        {
            TaskItemId = task.Id,
            ApplicationUserId = userId,
            AssignedDate = DateTime.Now,
            ViewState = true
        });
        context.SaveChanges();

        return (userId, task);
    }

    private static int GetBalance(ApplicationDbContext context, int userId)
        => context.Set<UserWallet>().First(w => w.UserId == userId).AvailablePoints;

    [Fact]
    public async Task RepeatedCompletion_DoesNotAwardPointsTwice()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var (userId, task) = SeedAssignedCompletedTask(context);
        var coordinator = CreateCoordinator(context);

        // تکمیل اول — باید پاداش بدهد
        await coordinator.HandleTaskCompletedAsync(
            task.Id, task.Title, new[] { userId }, TaskPriorityType.Medium, 4);
        var afterFirst = GetBalance(context, userId);

        // چرخه سوء‌استفاده: بازگرداندن به InProgress و تکمیل دوباره
        task.Status = TaskStatusType.InProgress;
        task.CompletedDate = null;
        await context.SaveChangesAsync();

        task.Status = TaskStatusType.Done;
        task.CompletedDate = DateTime.Now;
        await context.SaveChangesAsync();

        await coordinator.HandleTaskCompletedAsync(
            task.Id, task.Title, new[] { userId }, TaskPriorityType.Medium, 4);
        var afterSecond = GetBalance(context, userId);

        Assert.True(afterFirst > 0, "تکمیل اول باید امتیاز بدهد");
        Assert.Equal(afterFirst, afterSecond);
    }

    [Fact]
    public async Task RepeatedCompletion_DoesNotInflateTaskCounter()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var (userId, task) = SeedAssignedCompletedTask(context);
        var coordinator = CreateCoordinator(context);

        for (var i = 0; i < 5; i++)
        {
            await coordinator.HandleTaskCompletedAsync(
                task.Id, task.Title, new[] { userId }, TaskPriorityType.High, 8);
        }

        var progression = context.Set<UserProgression>().First(p => p.UserId == userId);

        // شمارنده باید فقط یک‌بار افزایش یابد، وگرنه دستاوردها هم قابل سوء‌استفاده‌اند
        Assert.Equal(1, progression.TasksCompleted);
    }

    [Fact]
    public async Task InstantlyCreatedTask_EarnsNothing()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        // تسک جعلی که برای گرفتن سکه ساخته و فوراً تکمیل شده
        var (userId, task) = SeedAssignedCompletedTask(context, lifetime: TimeSpan.FromSeconds(3));
        var coordinator = CreateCoordinator(context);

        await coordinator.HandleTaskCompletedAsync(
            task.Id, task.Title, new[] { userId }, TaskPriorityType.Highest, 1);

        Assert.Equal(0, GetBalance(context, userId));
    }

    [Fact]
    public async Task OnlyOneEarnedLedgerEntryExistsPerTask()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var (userId, task) = SeedAssignedCompletedTask(context);
        var coordinator = CreateCoordinator(context);

        await coordinator.HandleTaskCompletedAsync(
            task.Id, task.Title, new[] { userId }, TaskPriorityType.Medium, 4);
        await coordinator.HandleTaskCompletedAsync(
            task.Id, task.Title, new[] { userId }, TaskPriorityType.Medium, 4);

        var entries = context.Set<WalletTransaction>()
            .Count(t => t.RelatedTaskId == task.Id && t.TransactionType == TransactionType.Earned);

        Assert.Equal(1, entries);
    }
}
