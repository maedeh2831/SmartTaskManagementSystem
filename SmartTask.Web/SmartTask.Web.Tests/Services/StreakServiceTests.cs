/*
| Module      : Gamification Tests
| Class       : StreakServiceTests
| Purpose     : Unit tests for StreakService
*/

using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Gamification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace SmartTask.Web.Tests.Services
{
    public class StreakServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<ILogger<StreakService>> _loggerMock;
        private readonly StreakService _service;

        public StreakServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"streak_test_{Guid.NewGuid()}")
                .Options;

            _context = new ApplicationDbContext(options);
            _loggerMock = new Mock<ILogger<StreakService>>();
            _service = new StreakService(_context, _loggerMock.Object);
        }

        [Fact]
        public async Task GetCurrentStreakAsync_WithNewUser_ReturnsZero()
        {
            // Arrange
            int userId = 1;

            // Act
            var result = await _service.GetCurrentStreakAsync(userId);

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task UpdateStreakAsync_FirstCompletion_CreatesNewStreak()
        {
            // Arrange
            int userId = 1;
            int xpGained = 100;

            // Act
            await _service.UpdateStreakAsync(userId, xpGained);

            // Assert
            var streak = _context.Set<UserStreak>().FirstOrDefault(x => x.UserId == userId);
            Assert.NotNull(streak);
            Assert.Equal(1, streak.CurrentStreak);
            Assert.Equal(1, streak.LongestStreak);
            Assert.Equal(1, streak.TasksCompletedToday);
            Assert.Equal(xpGained, streak.XpGainedToday);
        }

        [Fact]
        public async Task UpdateStreakAsync_SameDayCompletion_IncrementsDailyStats()
        {
            // Arrange
            int userId = 2;
            var now = DateTime.UtcNow;
            var streak = new UserStreak
            {
                UserId = userId,
                CurrentStreak = 1,
                LongestStreak = 1,
                StreakStartDate = now,
                LastCompletionDate = now.AddHours(-1),
                TasksCompletedToday = 1,
                XpGainedToday = 100,
                UserTimeZone = "UTC",
                CreatedDate = now,
                CreatedBy = "test"
            };
            _context.Set<UserStreak>().Add(streak);
            _context.SaveChanges();

            // Act
            await _service.UpdateStreakAsync(userId, 50);

            // Assert
            var updated = _context.Set<UserStreak>().First(x => x.UserId == userId);
            Assert.Equal(1, updated.CurrentStreak);
            Assert.Equal(2, updated.TasksCompletedToday);
            Assert.Equal(150, updated.XpGainedToday);
        }

        [Fact]
        public async Task CheckMilestonesAsync_At3Days_AwardsMilestone()
        {
            // Arrange
            int userId = 3;
            var streak = new UserStreak
            {
                UserId = userId,
                CurrentStreak = 3,
                LongestStreak = 3,
                Milestone3Days = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<UserStreak>().Add(streak);
            _context.SaveChanges();

            // Act
            var (current, longest, milestonesReached) = await _service.CheckMilestonesAsync(userId);

            // Assert
            Assert.Equal(3, milestonesReached);
            var updated = _context.Set<UserStreak>().First(x => x.UserId == userId);
            Assert.True(updated.Milestone3Days);
        }

        [Fact]
        public async Task CheckMilestonesAsync_Multiple_AwardsAllApplicable()
        {
            // Arrange
            int userId = 4;
            var streak = new UserStreak
            {
                UserId = userId,
                CurrentStreak = 30,
                LongestStreak = 30,
                Milestone3Days = false,
                Milestone7Days = false,
                Milestone14Days = false,
                Milestone30Days = false,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<UserStreak>().Add(streak);
            _context.SaveChanges();

            // Act
            var (current, longest, milestonesReached) = await _service.CheckMilestonesAsync(userId);

            // Assert
            Assert.Equal(4, milestonesReached);
            var updated = _context.Set<UserStreak>().First(x => x.UserId == userId);
            Assert.True(updated.Milestone3Days);
            Assert.True(updated.Milestone7Days);
            Assert.True(updated.Milestone14Days);
            Assert.True(updated.Milestone30Days);
        }

        [Fact]
        public async Task ResetStreaksAsync_UpdatesAllStreaks()
        {
            // Arrange
            var now = DateTime.UtcNow;
            for (int i = 1; i <= 3; i++)
            {
                var streak = new UserStreak
                {
                    UserId = i,
                    CurrentStreak = i,
                    TasksCompletedToday = i,
                    XpGainedToday = i * 100,
                    LastResetDate = now.AddDays(-1),
                    CreatedDate = now,
                    CreatedBy = "test"
                };
                _context.Set<UserStreak>().Add(streak);
            }
            _context.SaveChanges();

            // Act
            await _service.ResetStreaksAsync();

            // Assert
            var streaks = _context.Set<UserStreak>().ToList();
            foreach (var streak in streaks)
            {
                Assert.Equal(0, streak.TasksCompletedToday);
                Assert.Equal(0, streak.XpGainedToday);
            }
        }

        [Fact]
        public async Task SetUserTimeZoneAsync_SetsCorrectTimeZone()
        {
            // Arrange
            int userId = 5;
            string timeZone = "Eastern Standard Time";

            // Act
            await _service.SetUserTimeZoneAsync(userId, timeZone);

            // Assert
            var streak = _context.Set<UserStreak>().First(x => x.UserId == userId);
            Assert.Equal(timeZone, streak.UserTimeZone);
        }
    }
}
