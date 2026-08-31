using Microsoft.Extensions.Logging;
using Moq;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Gamification;
using SmartTask.Web.Tests.TestHelpers;
using Xunit;

namespace SmartTask.Web.Tests.Services;

/// <summary>
/// دروازه ضد‌سوء‌استفاده: تسک تکراری، تسک جعلی، سقف نرخ و تعلیق
/// </summary>
public class RewardEligibilityServiceTests
{
    private static RewardEligibilityService CreateService(
        ApplicationDbContext context,
        bool suspended = false)
    {
        var abuseMock = new Mock<IAbuseDetectionEngine>();
        abuseMock.Setup(x => x.IsUserSuspendedAsync(It.IsAny<int>())).ReturnsAsync(suspended);

        return new RewardEligibilityService(
            context,
            abuseMock.Object,
            new Mock<ILogger<RewardEligibilityService>>().Object);
    }

    /// <summary>تسک تکمیل‌شده و تخصیص‌داده‌شده با سابقه معقول</summary>
    private static (int userId, int taskId) SeedCompletedTask(
        ApplicationDbContext context,
        string title = "یک تسک واقعی",
        TimeSpan? lifetime = null)
    {
        var userId = context.Set<ApplicationUser>().First().Id;
        var task = context.TaskItems.First();

        task.Title = title;
        task.Status = TaskStatusType.Done;
        task.CreatedDate = DateTime.Now - (lifetime ?? TimeSpan.FromHours(3));
        task.CompletedDate = DateTime.Now;

        context.Set<TaskAssignment>().Add(new TaskAssignment
        {
            TaskItemId = task.Id,
            ApplicationUserId = userId,
            AssignedDate = DateTime.Now,
            ViewState = true
        });
        context.SaveChanges();

        return (userId, task.Id);
    }

    private static void SeedWallet(ApplicationDbContext context, int userId, out int walletId, out int progressionId)
    {
        var wallet = new UserWallet { UserId = userId, LastUpdated = DateTime.UtcNow };
        var progression = new UserProgression { UserId = userId, LastProgressUpdate = DateTime.UtcNow };
        context.Set<UserWallet>().Add(wallet);
        context.Set<UserProgression>().Add(progression);
        context.SaveChanges();
        walletId = wallet.Id;
        progressionId = progression.Id;
    }

    [Fact]
    public async Task CanRewardTaskAsync_AllowsGenuineCompletion()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var (userId, taskId) = SeedCompletedTask(context);

        var result = await CreateService(context).CanRewardTaskAsync(userId, taskId);

        Assert.True(result.IsAllowed, result.Reason);
    }

    [Fact]
    public async Task CanRewardTaskAsync_DeniesSecondRewardForSameTask()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var (userId, taskId) = SeedCompletedTask(context);
        SeedWallet(context, userId, out var walletId, out var progressionId);

        // پاداش قبلی برای همین تسک ثبت شده است
        context.Set<WalletTransaction>().Add(new WalletTransaction
        {
            UserWalletId = walletId,
            UserProgressionId = progressionId,
            Amount = 150,
            TransactionType = TransactionType.Earned,
            Description = "تکمیل تسک",
            RelatedTaskId = taskId,
            TransactionDate = DateTime.UtcNow.AddMinutes(-5)
        });
        context.SaveChanges();

        var result = await CreateService(context).CanRewardTaskAsync(userId, taskId);

        // این همان چرخه Done → InProgress → Done است
        Assert.False(result.IsAllowed);
        Assert.Contains("قبلاً پاداش", result.Reason);
    }

    [Fact]
    public async Task CanRewardTaskAsync_DeniesWhenTaskNotDone()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var (userId, taskId) = SeedCompletedTask(context);

        var task = context.TaskItems.First(t => t.Id == taskId);
        task.Status = TaskStatusType.InProgress;
        context.SaveChanges();

        var result = await CreateService(context).CanRewardTaskAsync(userId, taskId);

        Assert.False(result.IsAllowed);
    }

    [Fact]
    public async Task CanRewardTaskAsync_DeniesWhenUserNotAssigned()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var (userId, taskId) = SeedCompletedTask(context);

        var result = await CreateService(context).CanRewardTaskAsync(userId + 999, taskId);

        Assert.False(result.IsAllowed);
    }

    [Fact]
    public async Task CanRewardTaskAsync_DeniesInstantlyCreatedAndCompletedTask()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        // تسک جعلی: ساخته و ۲ ثانیه بعد تکمیل شده
        var (userId, taskId) = SeedCompletedTask(context, lifetime: TimeSpan.FromSeconds(2));

        var result = await CreateService(context).CanRewardTaskAsync(userId, taskId);

        Assert.False(result.IsAllowed);
        Assert.Contains("بلافاصله", result.Reason);
    }

    [Fact]
    public async Task CanRewardTaskAsync_DeniesJunkTitle()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var (userId, taskId) = SeedCompletedTask(context, title: "a");

        var result = await CreateService(context).CanRewardTaskAsync(userId, taskId);

        Assert.False(result.IsAllowed);
    }

    [Fact]
    public async Task CanRewardTaskAsync_DeniesWhenHourlyCapExceeded()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var (userId, taskId) = SeedCompletedTask(context);
        SeedWallet(context, userId, out var walletId, out var progressionId);

        // ۲۰ پاداش در ساعت گذشته برای تسک‌های دیگر
        for (var i = 1; i <= 20; i++)
        {
            context.Set<WalletTransaction>().Add(new WalletTransaction
            {
                UserWalletId = walletId,
                UserProgressionId = progressionId,
                Amount = 100,
                TransactionType = TransactionType.Earned,
                Description = "پاداش",
                RelatedTaskId = 10_000 + i,
                TransactionDate = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        context.SaveChanges();

        var result = await CreateService(context).CanRewardTaskAsync(userId, taskId);

        Assert.False(result.IsAllowed);
        Assert.Contains("ساعتی", result.Reason);
    }

    [Fact]
    public async Task CanRewardTaskAsync_DeniesSuspendedUser()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var (userId, taskId) = SeedCompletedTask(context);

        var result = await CreateService(context, suspended: true).CanRewardTaskAsync(userId, taskId);

        Assert.False(result.IsAllowed);
        Assert.Contains("تعلیق", result.Reason);
    }

    [Fact]
    public async Task HasAlreadyRewardedTaskAsync_IsScopedPerUser()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var (userId, taskId) = SeedCompletedTask(context);
        SeedWallet(context, userId, out var walletId, out var progressionId);

        context.Set<WalletTransaction>().Add(new WalletTransaction
        {
            UserWalletId = walletId,
            UserProgressionId = progressionId,
            Amount = 100,
            TransactionType = TransactionType.Earned,
            Description = "پاداش",
            RelatedTaskId = taskId,
            TransactionDate = DateTime.UtcNow
        });
        context.SaveChanges();

        var service = CreateService(context);

        Assert.True(await service.HasAlreadyRewardedTaskAsync(userId, taskId));
        // کاربر دیگری که روی همان تسک کار کرده باید بتواند پاداش بگیرد
        Assert.False(await service.HasAlreadyRewardedTaskAsync(userId + 999, taskId));
    }
}
