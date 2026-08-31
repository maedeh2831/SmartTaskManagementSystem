/*
| Module      : Gamification
| Controller  : GamificationAdminController
| Purpose     : کنترل‌کننده مدیریت گیمیفیکیشن
*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Gamification;

namespace SmartTask.Web.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/gamification")]
    [Authorize(Roles = "Admin")]
    public class GamificationAdminController : ControllerBase
    {
        private readonly IGamificationAnalyticsService _analyticsService;
        private readonly IAbuseDetectionEngine _abuseEngine;
        private readonly ISeasonalEventService _seasonalService;
        private readonly IStreakService _streakService;
        private readonly ILogger<GamificationAdminController> _logger;

        public GamificationAdminController(
            IGamificationAnalyticsService analyticsService,
            IAbuseDetectionEngine abuseEngine,
            ISeasonalEventService seasonalService,
            IStreakService streakService,
            ILogger<GamificationAdminController> logger)
        {
            _analyticsService = analyticsService;
            _abuseEngine = abuseEngine;
            _seasonalService = seasonalService;
            _streakService = streakService;
            _logger = logger;
        }

        /// <summary>
        /// Get economy metrics dashboard
        /// </summary>
        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetrics()
        {
            try
            {
                var metrics = await _analyticsService.GetEconomyMetricsAsync();
                var levelDistribution = await _analyticsService.GetLevelDistributionAsync();
                var achievementRates = await _analyticsService.GetAchievementUnlockRatesAsync();

                return Ok(new
                {
                    metrics,
                    levelDistribution,
                    achievementRates
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics");
                return BadRequest("Failed to retrieve metrics");
            }
        }

        /// <summary>
        /// Get daily active users
        /// </summary>
        [HttpGet("daily-active-users")]
        public async Task<IActionResult> GetDailyActiveUsers([FromQuery] int days = 30)
        {
            try
            {
                var data = await _analyticsService.GetDailyActiveUsersAsync(days);
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily active users");
                return BadRequest("Failed to retrieve data");
            }
        }

        /// <summary>
        /// Get abuse reports
        /// </summary>
        [HttpGet("abuse-reports")]
        public async Task<IActionResult> GetAbuseReports([FromQuery] AbuseReportStatus? status = null)
        {
            try
            {
                var reports = await _abuseEngine.GetPendingReportsAsync();

                if (status.HasValue)
                {
                    reports = reports.Where(r => r.Status == status.Value).ToList();
                }

                return Ok(reports);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting abuse reports");
                return BadRequest("Failed to retrieve reports");
            }
        }

        /// <summary>
        /// Get specific abuse report
        /// </summary>
        [HttpGet("abuse-reports/{reportId}")]
        public async Task<IActionResult> GetAbuseReport(int reportId)
        {
            try
            {
                var report = await _abuseEngine.GetReportAsync(reportId);
                if (report == null)
                    return NotFound();

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting abuse report {ReportId}", reportId);
                return BadRequest("Failed to retrieve report");
            }
        }

        /// <summary>
        /// Resolve abuse report
        /// </summary>
        [HttpPost("abuse-reports/{reportId}/resolve")]
        public async Task<IActionResult> ResolveAbuseReport(int reportId, [FromBody] ResolveReportRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

                await _abuseEngine.ResolveReportAsync(reportId, request.Status, request.Notes, userId);

                if (request.RefundAmount > 0)
                {
                    await _abuseEngine.RefundRewardAsync(reportId, request.RefundAmount);
                }

                if (request.SuspendUntil.HasValue)
                {
                    await _abuseEngine.SuspendRewardsAsync(reportId, request.SuspendUntil.Value);
                }

                return Ok("Report resolved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving abuse report {ReportId}", reportId);
                return BadRequest("Failed to resolve report");
            }
        }

        /// <summary>
        /// Refund reward for user
        /// </summary>
        [HttpPost("users/{userId}/refund-reward")]
        public async Task<IActionResult> RefundReward(int userId, [FromBody] RefundRewardRequest request)
        {
            try
            {
                // Create temporary report for refund tracking
                var report = await _abuseEngine.GetPendingReportsAsync();
                var userReport = report.FirstOrDefault(r => r.UserId == userId);

                if (userReport == null)
                    return BadRequest("No abuse report found for this user");

                await _abuseEngine.RefundRewardAsync(userReport.Id, request.Amount);
                return Ok($"Refunded {request.Amount} points");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding reward for user {UserId}", userId);
                return BadRequest("Failed to refund reward");
            }
        }

        /// <summary>
        /// Suspend rewards for user
        /// </summary>
        [HttpPost("users/{userId}/suspend-rewards")]
        public async Task<IActionResult> SuspendRewards(int userId, [FromBody] SuspendRewardsRequest request)
        {
            try
            {
                var report = await _abuseEngine.GetPendingReportsAsync();
                var userReport = report.FirstOrDefault(r => r.UserId == userId);

                if (userReport == null)
                    return BadRequest("No abuse report found for this user");

                await _abuseEngine.SuspendRewardsAsync(userReport.Id, request.SuspendUntil);
                return Ok($"Rewards suspended until {request.SuspendUntil}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspending rewards for user {UserId}", userId);
                return BadRequest("Failed to suspend rewards");
            }
        }

        /// <summary>
        /// Get user progression details
        /// </summary>
        [HttpGet("users/{userId}/progression")]
        public async Task<IActionResult> GetUserProgression(int userId)
        {
            try
            {
                var progression = await _analyticsService.GetUserProgressionAdminAsync(userId);
                if (progression == null)
                    return NotFound();

                return Ok(progression);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user progression for user {UserId}", userId);
                return BadRequest("Failed to retrieve user progression");
            }
        }

        /// <summary>
        /// Get top users leaderboard
        /// </summary>
        [HttpGet("top-users")]
        public async Task<IActionResult> GetTopUsers([FromQuery] int limit = 20)
        {
            try
            {
                var topUsers = await _analyticsService.GetTopUsersAsync(limit);
                return Ok(topUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top users");
                return BadRequest("Failed to retrieve top users");
            }
        }

        /// <summary>
        /// Get seasonal events
        /// </summary>
        [HttpGet("seasonal-events")]
        public async Task<IActionResult> GetSeasonalEvents()
        {
            try
            {
                var events = await _seasonalService.GetActiveEventsAsync();
                return Ok(events);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting seasonal events");
                return BadRequest("Failed to retrieve events");
            }
        }

        /// <summary>
        /// Create seasonal event
        /// </summary>
        [HttpPost("seasonal-events")]
        public async Task<IActionResult> CreateSeasonalEvent([FromBody] dynamic eventData)
        {
            try
            {
                await _seasonalService.CreateEventAsync(eventData);
                return Ok("Event created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating seasonal event");
                return BadRequest("Failed to create event");
            }
        }

        /// <summary>
        /// Get marketplace metrics
        /// </summary>
        [HttpGet("marketplace-metrics")]
        public async Task<IActionResult> GetMarketplaceMetrics()
        {
            try
            {
                var metrics = await _analyticsService.GetMarketplaceMetricsAsync();
                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting marketplace metrics");
                return BadRequest("Failed to retrieve metrics");
            }
        }

        /// <summary>
        /// Force streak reset
        /// </summary>
        [HttpPost("streaks/reset")]
        public async Task<IActionResult> ResetStreaks()
        {
            try
            {
                await _streakService.ResetStreaksAsync();
                return Ok("Streaks reset successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting streaks");
                return BadRequest("Failed to reset streaks");
            }
        }
    }

    // Request models
    public class ResolveReportRequest
    {
        public AbuseReportStatus Status { get; set; }
        public string Notes { get; set; }
        public int RefundAmount { get; set; } = 0;
        public DateTime? SuspendUntil { get; set; }
    }

    public class RefundRewardRequest
    {
        public int Amount { get; set; }
    }

    public class SuspendRewardsRequest
    {
        public DateTime SuspendUntil { get; set; }
    }
}
