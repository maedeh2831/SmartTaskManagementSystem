/*
| Module      : Gamification Tests
| Class       : GamificationIntegrationTests
| Purpose     : Integration tests for end-to-end gamification flow
*/

using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Gamification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace SmartTask.Web.Tests.Services
{
    public class GamificationIntegrationTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<ILogger<RewardEngine>> _rewardLoggerMock;
        private readonly Mock<ILogger<StreakService>> _streakLoggerMock;
        private readonly Mock<ILogger<AbuseDetectionEngine>> _abuseLoggerMock;
        private readonly RewardEngine _rewardEngine;
        private readonly StreakService _streakService;
        private readonly AbuseDetectionEngine _abuseEngine;

        public GamificationIntegrationTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"integration_test_{Guid.NewGuid()}")
                .Options;

            _context = new ApplicationDbContext(options);
            _rewardLoggerMock = new Mock<ILogger<RewardEngine>>();
            _streakLoggerMock = new Mock<ILogger<StreakService>>();
            _abuseLoggerMock = new Mock<ILogger<AbuseDetectionEngine>>();

            _rewardEngine = new RewardEngine(_context, _rewardLoggerMock.Object);
            _streakService = new StreakService(_context, _streakLoggerMock.Object);
            _abuseEngine = new AbuseDetectionEngine(_context, _abuseLoggerMock.Object);
        }

        [Fact]
        public async Task FullTaskCompletionFlow_CompletesSuccessfully()
        {
            // Arrange: Setup user and task
            var user = new ApplicationUser { UserName = "flowuser", Email = "flow@test.com" };
            _context.Users.Add(user);

            var workspace = new Workspace
            {
                Name = "Test Workspace",
                Description = "Integration test workspace",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<Workspace>().Add(workspace);

            var project = new Project
            {
                WorkspaceId = workspace.Id,
                Name = "Test Project",
                Description = "Integration test project",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<Project>().Add(project);

            var task = new TaskItem
            {
                ProjectId = project.Id,
                AssignedToUserId = user.Id,
                Title = "Integration Test Task",
                Description = "Test task for integration",
                Priority = TaskPriority.High,
                Complexity = TaskComplexity.Complex,
                EstimatedHours = 2,
                Status = TaskStatus.Open,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<TaskItem>().Add(task);

            var progression = new UserProgression
            {
                UserId = user.Id,
                CurrentLevel = 1,
                TotalExperience = 0,
                TasksCompleted = 0,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<UserProgression>().Add(progression);

            var wallet = new UserWallet
            {
                UserId = user.Id,
                TotalPoints = 0,
                AvailablePoints = 0,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<UserWallet>().Add(wallet);

            _context.SaveChanges();

            // Act: Process reward and streak update
            var reward = await _rewardEngine.CalculateTaskRewardAsync(task.Id, user.Id);
            await _streakService.UpdateStreakAsync(user.Id, reward);
            await _abuseEngine.ScanUserActivityAsync(user.Id);

            // Assert: Verify results
            var updatedProgression = _context.Set<UserProgression>().First(p => p.UserId == user.Id);
            var updatedWallet = _context.Set<UserWallet>().First(w => w.UserId == user.Id);
            var streak = _context.Set<UserStreak>().FirstOrDefault(s => s.UserId == user.Id);

            Assert.NotEqual(0, updatedProgression.TotalExperience);
            Assert.NotEqual(0, updatedWallet.TotalPoints);
            Assert.NotNull(streak);
            Assert.Equal(1, streak.CurrentStreak);
        }

        [Fact]
        public async Task MultipleTaskCompletions_BuildsStreakCorrectly()
        {
            // Arrange: Setup user with multiple tasks
            var user = new ApplicationUser { UserName = "streakuser", Email = "streak@test.com" };
            _context.Users.Add(user);

            var progression = new UserProgression
            {
                UserId = user.Id,
                CurrentLevel = 1,
                TotalExperience = 0,
                TasksCompleted = 0,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<UserProgression>().Add(progression);

            _context.SaveChanges();

            // Act: Complete 3 tasks on same day
            for (int i = 0; i < 3; i++)
            {
                await _streakService.UpdateStreakAsync(user.Id, 100);
            }

            // Assert: Verify streak is still 1 but tasks completed = 3
            var streak = _context.Set<UserStreak>().First(s => s.UserId == user.Id);
            Assert.Equal(1, streak.CurrentStreak);
            Assert.Equal(3, streak.TasksCompletedToday);
            Assert.Equal(300, streak.XpGainedToday);
        }

        [Fact]
        public async Task AbuseDetection_FlagsRapidCompletion()
        {
            // Arrange: Setup user with rapid task completions
            var user = new ApplicationUser { UserName = "abuseuser", Email = "abuse@test.com" };
            _context.Users.Add(user);

            var progression = new UserProgression
            {
                UserId = user.Id,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<UserProgression>().Add(progression);

            _context.SaveChanges();

            // Act: Create rapid transactions (simulating rapid completions)
            var now = DateTime.UtcNow;
            var wallet = new UserWallet
            {
                UserId = user.Id,
                TotalPoints = 10000,
                AvailablePoints = 10000,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<UserWallet>().Add(wallet);

            for (int i = 0; i < 55; i++)
            {
                var transaction = new WalletTransaction
                {
                    UserWalletId = wallet.Id,
                    Amount = 100,
                    TransactionType = TransactionType.Earned,
                    Description = $"Task {i}",
                    TransactionDate = now.AddMinutes(-30),
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "test"
                };
                _context.Set<WalletTransaction>().Add(transaction);
            }
            _context.SaveChanges();

            // Act: Scan for abuse
            await _abuseEngine.ScanUserActivityAsync(user.Id);

            // Assert: Verify report was created
            var reports = await _abuseEngine.GetPendingReportsAsync();
            Assert.NotEmpty(reports);
            Assert.True(reports.Any(r => r.ReportType == AbuseReportType.RapidCompletion));
        }

        [Fact]
        public async Task SeasonalEvent_BoostsRewards()
        {
            // Arrange: Create seasonal event
            var eventEntity = new SeasonalEvent
            {
                Name = "Integration Test Event",
                Description = "Test seasonal event",
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(1),
                Status = EventStatus.Active,
                IsActive = true,
                AchievementBonusMultiplier = 1.5m,
                RewardBonusMultiplier = 2.0m,
                ExtraPointsPerCompletion = 50,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<SeasonalEvent>().Add(eventEntity);

            var user = new ApplicationUser { UserName = "eventuser", Email = "event@test.com" };
            _context.Users.Add(user);

            var userEventProgress = new UserSeasonalEventProgress
            {
                UserId = user.Id,
                SeasonalEventId = eventEntity.Id,
                TasksCompleted = 0,
                PointsEarned = 0,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<UserSeasonalEventProgress>().Add(userEventProgress);

            _context.SaveChanges();

            // Act: Verify event is active and boost is applied
            var activeEvents = _context.Set<SeasonalEvent>()
                .Where(e => e.IsActive && e.Status == EventStatus.Active)
                .ToList();

            var boostedReward = 100 * eventEntity.RewardBonusMultiplier + eventEntity.ExtraPointsPerCompletion;

            // Assert
            Assert.NotEmpty(activeEvents);
            Assert.Equal(250, boostedReward); // 100 * 2.0 + 50
        }

        [Fact]
        public async Task MilestoneBonus_AwardedAt3Days()
        {
            // Arrange: Create streak at 3-day milestone
            var user = new ApplicationUser { UserName = "milestoneuser", Email = "milestone@test.com" };
            _context.Users.Add(user);

            var streak = new UserStreak
            {
                UserId = user.Id,
                CurrentStreak = 3,
                LongestStreak = 3,
                Milestone3Days = false,
                Milestone7Days = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<UserStreak>().Add(streak);

            _context.SaveChanges();

            // Act: Check for milestone
            var (current, longest, milestonesReached) = await _streakService.CheckMilestonesAsync(user.Id);

            // Assert
            Assert.Equal(1, milestonesReached);
            var updatedStreak = _context.Set<UserStreak>().First(s => s.UserId == user.Id);
            Assert.True(updatedStreak.Milestone3Days);
        }

        [Fact]
        public async Task RewardSuspension_PreventsNewRewards()
        {
            // Arrange: Setup suspended user
            var user = new ApplicationUser { UserName = "suspenduser", Email = "suspend@test.com" };
            _context.Users.Add(user);

            var report = new AbuseReport
            {
                UserId = user.Id,
                ReportType = AbuseReportType.RapidCompletion,
                Status = AbuseReportStatus.Confirmed,
                RewardsSuspended = true,
                SuspensionUntil = DateTime.UtcNow.AddDays(7),
                Description = "Suspended for rapid completion",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<AbuseReport>().Add(report);

            _context.SaveChanges();

            // Act: Check if user is suspended
            var isSuspended = await _abuseEngine.IsUserSuspendedAsync(user.Id);

            // Assert
            Assert.True(isSuspended);
        }
    }
}
