/*
| Module      : Gamification Tests
| Class       : AbuseDetectionEngineTests
| Purpose     : Unit tests for AbuseDetectionEngine
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
    public class AbuseDetectionEngineTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<ILogger<AbuseDetectionEngine>> _loggerMock;
        private readonly AbuseDetectionEngine _service;

        public AbuseDetectionEngineTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"abuse_test_{Guid.NewGuid()}")
                .Options;

            _context = new ApplicationDbContext(options);
            _loggerMock = new Mock<ILogger<AbuseDetectionEngine>>();
            _service = new AbuseDetectionEngine(_context, _loggerMock.Object);
        }

        [Fact]
        public async Task GetPendingReportsAsync_ReturnsPendingReports()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "testuser", Email = "test@test.com" };
            _context.Users.Add(user);
            _context.SaveChanges();

            var reports = new List<AbuseReport>
            {
                new AbuseReport
                {
                    UserId = user.Id,
                    ReportType = AbuseReportType.RapidCompletion,
                    Status = AbuseReportStatus.Pending,
                    Description = "Test",
                    SeverityScore = 80,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "test"
                },
                new AbuseReport
                {
                    UserId = user.Id,
                    ReportType = AbuseReportType.VelocityAnomaly,
                    Status = AbuseReportStatus.Pending,
                    Description = "Test 2",
                    SeverityScore = 60,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "test"
                }
            };
            _context.Set<AbuseReport>().AddRange(reports);
            _context.SaveChanges();

            // Act
            var result = await _service.GetPendingReportsAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(80, result[0].SeverityScore); // Sorted by severity descending
        }

        [Fact]
        public async Task GetReportAsync_ReturnsCorrectReport()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "testuser2", Email = "test2@test.com" };
            _context.Users.Add(user);
            _context.SaveChanges();

            var report = new AbuseReport
            {
                UserId = user.Id,
                ReportType = AbuseReportType.SystemManipulation,
                Status = AbuseReportStatus.Pending,
                Description = "Test report",
                Evidence = "{\"timestamp\":\"mismatch\"}",
                SeverityScore = 90,
                ConfidenceLevel = 0.95m,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<AbuseReport>().Add(report);
            _context.SaveChanges();

            // Act
            var result = await _service.GetReportAsync(report.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.UserId);
            Assert.Equal(AbuseReportType.SystemManipulation, result.ReportType);
        }

        [Fact]
        public async Task ResolveReportAsync_UpdatesReportStatus()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "testuser3", Email = "test3@test.com" };
            _context.Users.Add(user);
            _context.SaveChanges();

            var report = new AbuseReport
            {
                UserId = user.Id,
                ReportType = AbuseReportType.DuplicateCompletions,
                Status = AbuseReportStatus.Pending,
                Description = "Test",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<AbuseReport>().Add(report);
            _context.SaveChanges();

            // Act
            await _service.ResolveReportAsync(report.Id, AbuseReportStatus.Confirmed, "Confirmed suspicious activity", user.Id);

            // Assert
            var updated = _context.Set<AbuseReport>().First(x => x.Id == report.Id);
            Assert.Equal(AbuseReportStatus.Confirmed, updated.Status);
            Assert.Equal("Confirmed suspicious activity", updated.ReviewNotes);
            Assert.Equal(user.Id, updated.ReviewedByUserId);
        }

        [Fact]
        public async Task RefundRewardAsync_ReducesUserWallet()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "testuser4", Email = "test4@test.com" };
            _context.Users.Add(user);
            _context.SaveChanges();

            var wallet = new UserWallet
            {
                UserId = user.Id,
                AvailablePoints = 500,
                TotalPoints = 1000,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<UserWallet>().Add(wallet);

            var report = new AbuseReport
            {
                UserId = user.Id,
                ReportType = AbuseReportType.LowEstimateTaskFarming,
                Status = AbuseReportStatus.Confirmed,
                Description = "Test",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<AbuseReport>().Add(report);
            _context.SaveChanges();

            // Act
            await _service.RefundRewardAsync(report.Id, 200);

            // Assert
            var updated = _context.Set<UserWallet>().First(x => x.UserId == user.Id);
            Assert.Equal(300, updated.AvailablePoints);
            Assert.Equal(800, updated.TotalPoints);
            Assert.True(_context.Set<AbuseReport>().First(x => x.Id == report.Id).RewardsRefunded);
        }

        [Fact]
        public async Task SuspendRewardsAsync_SetsSuspensionUntil()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "testuser5", Email = "test5@test.com" };
            _context.Users.Add(user);
            _context.SaveChanges();

            var report = new AbuseReport
            {
                UserId = user.Id,
                ReportType = AbuseReportType.RapidCompletion,
                Status = AbuseReportStatus.Confirmed,
                Description = "Test",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<AbuseReport>().Add(report);
            _context.SaveChanges();

            var suspendUntil = DateTime.UtcNow.AddDays(7);

            // Act
            await _service.SuspendRewardsAsync(report.Id, suspendUntil);

            // Assert
            var updated = _context.Set<AbuseReport>().First(x => x.Id == report.Id);
            Assert.True(updated.RewardsSuspended);
            Assert.Equal(suspendUntil, updated.SuspensionUntil);
        }

        [Fact]
        public async Task IsUserSuspendedAsync_ReturnsTrueForActiveSuspension()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "testuser6", Email = "test6@test.com" };
            _context.Users.Add(user);
            _context.SaveChanges();

            var report = new AbuseReport
            {
                UserId = user.Id,
                ReportType = AbuseReportType.VelocityAnomaly,
                Status = AbuseReportStatus.Confirmed,
                Description = "Test",
                RewardsSuspended = true,
                SuspensionUntil = DateTime.UtcNow.AddDays(3),
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<AbuseReport>().Add(report);
            _context.SaveChanges();

            // Act
            var result = await _service.IsUserSuspendedAsync(user.Id);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsUserSuspendedAsync_ReturnsFalseForExpiredSuspension()
        {
            // Arrange
            var user = new ApplicationUser { UserName = "testuser7", Email = "test7@test.com" };
            _context.Users.Add(user);
            _context.SaveChanges();

            var report = new AbuseReport
            {
                UserId = user.Id,
                ReportType = AbuseReportType.DuplicateCompletions,
                Status = AbuseReportStatus.Confirmed,
                Description = "Test",
                RewardsSuspended = true,
                SuspensionUntil = DateTime.UtcNow.AddDays(-1), // Expired
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "test"
            };
            _context.Set<AbuseReport>().Add(report);
            _context.SaveChanges();

            // Act
            var result = await _service.IsUserSuspendedAsync(user.Id);

            // Assert
            Assert.False(result);
        }
    }
}
