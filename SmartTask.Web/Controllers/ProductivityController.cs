/*
| Module      : Controllers
| Controller  : ProductivityController
| Purpose     : مدیریت API های متریکس بهره‌وری
*/

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Services.Gamification;

namespace SmartTask.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductivityController : ControllerBase
    {
        private readonly IProductivityMetricsService _productivityService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ProductivityController> _logger;

        public ProductivityController(
            IProductivityMetricsService productivityService,
            ICurrentUserService currentUserService,
            ILogger<ProductivityController> logger)
        {
            _productivityService = productivityService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        /// <summary>
        /// دریافت متریکس بهره‌وری جاری کاربر
        /// GET /api/productivity/user/{userId}
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserProductivity(int userId)
        {
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (currentUserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var metrics = await _productivityService.GetUserProductivityAsync(userId);
                if (metrics == null)
                {
                    return NotFound(new { message = "Productivity metrics not found" });
                }

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving productivity metrics for user {UserId}", userId);
                return StatusCode(500, new { message = "Error retrieving productivity metrics" });
            }
        }

        /// <summary>
        /// دریافت تاریخچهٔ نمرات بهره‌وری
        /// GET /api/productivity/user/{userId}/history?days=30
        /// </summary>
        [HttpGet("user/{userId}/history")]
        public async Task<IActionResult> GetProductivityHistory(int userId, int days = 30)
        {
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (currentUserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                if (days < 1 || days > 365)
                    days = 30;

                var history = await _productivityService.GetScoreHistoryAsync(userId, days);

                return Ok(new
                {
                    userId,
                    days,
                    data = history
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving productivity history for user {UserId}", userId);
                return StatusCode(500, new { message = "Error retrieving productivity history" });
            }
        }

        /// <summary>
        /// دریافت متریکس بهره‌وری تیم
        /// GET /api/productivity/team/{teamId}
        /// </summary>
        [HttpGet("team/{teamId}")]
        public async Task<IActionResult> GetTeamProductivity(int teamId)
        {
            try
            {
                var teamMetrics = await _productivityService.GetTeamProductivityAsync(teamId);

                return Ok(teamMetrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving team productivity for team {TeamId}", teamId);
                return StatusCode(500, new { message = "Error retrieving team productivity" });
            }
        }

        /// <summary>
        /// دریافت معیارهای تطابق (کاربر vs تیم vs workspace)
        /// GET /api/productivity/benchmarks
        /// </summary>
        [HttpGet("benchmarks")]
        public async Task<IActionResult> GetBenchmarkMetrics()
        {
            try
            {
                var userId = _currentUserService.UserId;
                var benchmarks = await _productivityService.GetBenchmarkMetricsAsync(userId);

                if (benchmarks == null)
                {
                    return NotFound(new { message = "Benchmark metrics not found" });
                }

                return Ok(benchmarks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving benchmark metrics for current user");
                return StatusCode(500, new { message = "Error retrieving benchmark metrics" });
            }
        }

        /// <summary>
        /// دریافت نمرهٔ بهره‌وری فوری کاربر
        /// GET /api/productivity/user/{userId}/score
        /// </summary>
        [HttpGet("user/{userId}/score")]
        public async Task<IActionResult> GetProductivityScore(int userId)
        {
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (currentUserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var score = await _productivityService.CalculateProductivityScoreAsync(userId);
                var tier = await _productivityService.GetProductivityTierAsync(userId);

                return Ok(new
                {
                    userId,
                    productivityScore = score,
                    tier = (int)tier,
                    tierName = tier.ToString()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating productivity score for user {UserId}", userId);
                return StatusCode(500, new { message = "Error calculating productivity score" });
            }
        }

        /// <summary>
        /// دریافت نمرهٔ تکمیل کارها
        /// GET /api/productivity/user/{userId}/completion-rate
        /// </summary>
        [HttpGet("user/{userId}/completion-rate")]
        public async Task<IActionResult> GetTaskCompletionRate(int userId)
        {
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (currentUserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var rate = await _productivityService.CalculateTaskCompletionRateAsync(userId);

                return Ok(new
                {
                    userId,
                    taskCompletionRate = rate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task completion rate for user {UserId}", userId);
                return StatusCode(500, new { message = "Error retrieving completion rate" });
            }
        }

        /// <summary>
        /// دریافت نمرهٔ تحویل به‌موقع
        /// GET /api/productivity/user/{userId}/on-time-rate
        /// </summary>
        [HttpGet("user/{userId}/on-time-rate")]
        public async Task<IActionResult> GetOnTimeDeliveryRate(int userId)
        {
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (currentUserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var rate = await _productivityService.CalculateOnTimeDeliveryRateAsync(userId);

                return Ok(new
                {
                    userId,
                    onTimeDeliveryRate = rate
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving on-time delivery rate for user {UserId}", userId);
                return StatusCode(500, new { message = "Error retrieving on-time delivery rate" });
            }
        }

        /// <summary>
        /// دریافت رشتهٔ فعالیت کاربر
        /// GET /api/productivity/user/{userId}/streak
        /// </summary>
        [HttpGet("user/{userId}/streak")]
        public async Task<IActionResult> GetUserStreak(int userId)
        {
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (currentUserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var currentStreak = await _productivityService.UpdateStreakAsync(userId);

                return Ok(new
                {
                    userId,
                    currentStreak
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving streak for user {UserId}", userId);
                return StatusCode(500, new { message = "Error retrieving streak" });
            }
        }

        /// <summary>
        /// دریافت داشبورد بهره‌وری جامع
        /// GET /api/productivity/dashboard/{userId}
        /// </summary>
        [HttpGet("dashboard/{userId}")]
        public async Task<IActionResult> GetProductivityDashboard(int userId)
        {
            try
            {
                var currentUserId = _currentUserService.UserId;
                if (currentUserId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var userMetrics = await _productivityService.GetUserProductivityAsync(userId);
                var benchmarks = await _productivityService.GetBenchmarkMetricsAsync(userId);
                var history = await _productivityService.GetScoreHistoryAsync(userId, 30);

                var dashboard = new
                {
                    userMetrics,
                    benchmarks,
                    recentHistory = history.Take(10)
                };

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving productivity dashboard for user {UserId}", userId);
                return StatusCode(500, new { message = "Error retrieving productivity dashboard" });
            }
        }
    }
}
