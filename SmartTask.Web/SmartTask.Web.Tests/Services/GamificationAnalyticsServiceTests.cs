/*
| Module      : Gamification Tests
| Class       : GamificationAnalyticsServiceTests
| Purpose     : Unit tests for GamificationAnalyticsService
*/

using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.ViewModels.Gamification.Admin;
using SmartTask.Web.Services.Gamification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace SmartTask.Web.Tests.Services
{
    public class GamificationAnalyticsServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<ILogger<GamificationAnalyticsService>> _loggerMock;
        private readonly GamificationAnalyticsService _service;

        public GamificationAnalyticsServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"analytics_test_{Guid.NewGuid()}")
                .Options;

            _context = new ApplicationDbContext(options);
            _loggerMock = new Mock<ILogger<GamificationAnalyticsService>>();
            _service = new GamificationAnalyticsService(_context, _loggerMock.Object);
        }

        [Fact]
        public async Task GetEconomyMetricsAsync_ReturnsValidMetrics()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "user1", Email = "user1@test.com" };
            _context.Users.Add(user);
            _context.SaveChanges();

            var progression = new UserProgression
            {
                UserId = user.Id,
                CurrentLevel = 5,
                TotalExperience = 5000,
                TasksCompleted = 50,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<UserProgression>().Add(progression);

            var wallet = new UserWallet
            {
                UserId = user.Id,
                TotalPoints = 2000,
                AvailablePoints = 1500,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<UserWallet>().Add(wallet);

            var achievement = new Achievement
            {
                Name = "First Task",
                Description = "Complete first task",
                XpReward = 100,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<Achievement>().Add(achievement);

            var userAchievement = new UserAchievement
            {
                UserId = user.Id,
                AchievementId = achievement.Id,
                UnlockedDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<UserAchievement>().Add(userAchievement);

            _context.SaveChanges();

            // Act
            var metrics = await _service.GetEconomyMetricsAsync();

            // Assert
            Assert.NotNull(metrics);
            Assert.Equal(5000, metrics.TotalXpDistributed);
            Assert.Equal(2000, metrics.TotalMomentumCirculating);
            Assert.Equal(1, metrics.TotalAchievementsUnlocked);
        }

        [Fact]
        public async Task GetDailyActiveUsersAsync_ReturnsCorrectCount()
        {
            // Arrange
            var now = DateTime.UtcNow;
            for (int i = 0; i < 3; i++)
            {
                var log = new ActivityLog
                {
                    Activity = "TaskCompleted",
                    CreatedDate = now.AddDays(-i),
                    CreatedBy = $"user{i}"
                };
                _context.Set<ActivityLog>().Add(log);
            }
            _context.SaveChanges();

            // Act
            var result = await _service.GetDailyActiveUsersAsync(30);

            // Assert
            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GetLevelDistributionAsync_ReturnsDistribution()
        {
            // Arrange
            for (int level = 1; level <= 10; level++)
            {
                var leaderboard = new Leaderboard
                {
                    UserId = level,
                    CurrentLevel = level,
                    GlobalRank = level,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "test"
                };
                _context.Set<Leaderboard>().Add(leaderboard);
            }
            _context.SaveChanges();

            // Act
            var distribution = await _service.GetLevelDistributionAsync();

            // Assert
            Assert.NotEmpty(distribution);
            Assert.Equal(10, distribution.Count);
        }

        [Fact]
        public async Task GetAchievementUnlockRatesAsync_CalculatesPercentages()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "user2", Email = "user2@test.com" };
            _context.Users.Add(user);

            var progression = new UserProgression
            {
                UserId = user.Id,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<UserProgression>().Add(progression);

            var achievement = new Achievement
            {
                Name = "Achievement 1",
                Description = "Test achievement",
                XpReward = 50,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<Achievement>().Add(achievement);

            var userAchievement = new UserAchievement
            {
                UserId = user.Id,
                AchievementId = achievement.Id,
                UnlockedDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<UserAchievement>().Add(userAchievement);

            _context.SaveChanges();

            // Act
            var rates = await _service.GetAchievementUnlockRatesAsync();

            // Assert
            Assert.NotEmpty(rates);
            Assert.Equal(1, rates[0].UnlockCount);
        }

        [Fact]
        public async Task GetUserProgressionAdminAsync_ReturnsCompleteUserData()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "admin_user", Email = "admin@test.com" };
            _context.Users.Add(user);

            var progression = new UserProgression
            {
                UserId = user.Id,
                CurrentLevel = 10,
                TotalExperience = 10000,
                TasksCompleted = 100,
                ProjectsCompleted = 5,
                AchievementsUnlocked = 20,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<UserProgression>().Add(progression);

            var wallet = new UserWallet
            {
                UserId = user.Id,
                TotalPoints = 5000,
                AvailablePoints = 3000,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<UserWallet>().Add(wallet);

            var leaderboard = new Leaderboard
            {
                UserId = user.Id,
                GlobalRank = 1,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<Leaderboard>().Add(leaderboard);

            var streak = new UserStreak
            {
                UserId = user.Id,
                CurrentStreak = 15,
                LongestStreak = 30,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<UserStreak>().Add(streak);

            _context.SaveChanges();

            // Act
            var result = await _service.GetUserProgressionAdminAsync(user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(10, result.Level);
            Assert.Equal(100, result.TasksCompleted);
            Assert.Equal(15, result.CurrentStreak);
            Assert.Equal(1, result.GlobalRank);
        }

        [Fact]
        public async Task GetTopUsersAsync_ReturnsLimitedResults()
        {
            // Arrange
            for (int i = 1; i <= 25; i++)
            {
                var user = new ApplicationUser { UserName = $"topuser{i}", Email = $"topuser{i}@test.com" };
                _context.Users.Add(user);
                _context.SaveChanges();

                var leaderboard = new Leaderboard
                {
                    UserId = user.Id,
                    GlobalRank = i,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "test"
                };
                _context.Set<Leaderboard>().Add(leaderboard);
            }
            _context.SaveChanges();

            // Act
            var topUsers = await _service.GetTopUsersAsync(limit: 10);

            // Assert
            Assert.Equal(10, topUsers.Count);
        }
    }
}
