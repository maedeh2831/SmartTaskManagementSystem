/*
| Module      : Gamification Services
| Service     : ProductivityMetricsService
| Purpose     : محاسبهٔ متریکس بهره‌وری و نمرات کاربران
*/

using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Gamification;

namespace SmartTask.Web.Services.Gamification
{
    public class ProductivityMetricsService : IProductivityMetricsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ProductivityMetricsService> _logger;

        public ProductivityMetricsService(ApplicationDbContext context, ILogger<ProductivityMetricsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// محاسبهٔ نمرهٔ بهره‌وری (فرمول جامع)
        /// Base: Task Completion Rate (40%)
        /// On-Time Delivery (35%)
        /// Consistency (15%) - worked_days / total_days
        /// Quality (10%) - tasks without reopens
        /// Result: 0-100
        /// </summary>
        public async Task<double> CalculateProductivityScoreAsync(int userId, string period = "month")
        {
            try
            {
                var completionRate = await CalculateTaskCompletionRateAsync(userId, period);
                var onTimeRate = await CalculateOnTimeDeliveryRateAsync(userId, period);
                var consistencyRate = await CalculateConsistencyRateAsync(userId, period);
                var qualityScore = await CalculateQualityScoreAsync(userId, period);

                // Formula: weighted average
                double score = (completionRate * 0.40) +
                               (onTimeRate * 0.35) +
                               (consistencyRate * 0.15) +
                               (qualityScore * 0.10);

                // Clamp between 0-100
                return Math.Round(Math.Min(100, Math.Max(0, score)), 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating productivity score for user {UserId}", userId);
                return 0;
            }
        }

        /// <summary>
        /// محاسبهٔ درصد تکمیل کارها
        /// </summary>
        public async Task<double> CalculateTaskCompletionRateAsync(int userId, string period = "month")
        {
            try
            {
                var dateRange = GetDateRange(period);

                var assignedTasks = await _context.Set<TaskItem>()
                    .Where(t => t.Assignments.Any(a => a.ApplicationUserId == userId) &&
                                t.CreatedDate >= dateRange.start &&
                                t.CreatedDate <= dateRange.end)
                    .CountAsync();

                if (assignedTasks == 0)
                    return 0;

                var completedTasks = await _context.Set<TaskItem>()
                    .Where(t => t.Assignments.Any(a => a.ApplicationUserId == userId) &&
                                t.CreatedDate >= dateRange.start &&
                                t.CreatedDate <= dateRange.end &&
                                t.Status == TaskStatusType.Done)
                    .CountAsync();

                double rate = (double)completedTasks / assignedTasks * 100;
                return Math.Round(Math.Min(100, rate), 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating task completion rate for user {UserId}", userId);
                return 0;
            }
        }

        /// <summary>
        /// محاسبهٔ درصد تحویل به‌موقع
        /// </summary>
        public async Task<double> CalculateOnTimeDeliveryRateAsync(int userId, string period = "month")
        {
            try
            {
                var dateRange = GetDateRange(period);

                var completedTasks = await _context.Set<TaskItem>()
                    .Where(t => t.Assignments.Any(a => a.ApplicationUserId == userId) &&
                                t.CreatedDate >= dateRange.start &&
                                t.CreatedDate <= dateRange.end &&
                                t.Status == TaskStatusType.Done)
                    .ToListAsync();

                if (completedTasks.Count == 0)
                    return 0;

                var onTimeTasks = completedTasks
                    .Where(t => t.DueDate == null || t.DueDate >= t.ChangeDate)
                    .Count();

                double rate = (double)onTimeTasks / completedTasks.Count * 100;
                return Math.Round(Math.Min(100, rate), 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating on-time delivery rate for user {UserId}", userId);
                return 0;
            }
        }

        /// <summary>
        /// محاسبهٔ نمرهٔ کیفیت (کارهای بدون بازگشایی)
        /// </summary>
        public async Task<double> CalculateQualityScoreAsync(int userId, string period = "month")
        {
            try
            {
                var dateRange = GetDateRange(period);

                var completedTasks = await _context.Set<TaskItem>()
                    .Where(t => t.Assignments.Any(a => a.ApplicationUserId == userId) &&
                                t.CreatedDate >= dateRange.start &&
                                t.CreatedDate <= dateRange.end &&
                                t.Status == TaskStatusType.Done)
                    .ToListAsync();

                if (completedTasks.Count == 0)
                    return 0;

                // For now, we'll consider tasks without reopens as quality metric
                // This would require tracking task state changes
                var qualityTasks = completedTasks.Count; // Simplified - needs enhancement

                double score = (double)qualityTasks / completedTasks.Count * 100;
                return Math.Round(Math.Min(100, score), 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating quality score for user {UserId}", userId);
                return 0;
            }
        }

        /// <summary>
        /// محاسبهٔ درصد سازگاری
        /// </summary>
        public async Task<double> CalculateConsistencyRateAsync(int userId, string period = "month")
        {
            try
            {
                var dateRange = GetDateRange(period);
                var totalDays = (int)(dateRange.end - dateRange.start).TotalDays;

                // Count days user had activity (task changes, comments, etc)
                var activeDays = await _context.Set<ActivityLog>()
                    .Where(a => a.ApplicationUserId == userId &&
                                a.CreatedDate >= dateRange.start &&
                                a.CreatedDate <= dateRange.end)
                    .Select(a => a.CreatedDate.Date)
                    .Distinct()
                    .CountAsync();

                if (totalDays == 0)
                    return 0;

                double rate = (double)activeDays / totalDays * 100;
                return Math.Round(Math.Min(100, rate), 2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating consistency rate for user {UserId}", userId);
                return 0;
            }
        }

        /// <summary>
        /// به‌روزرسانی رشتهٔ فعالیت کاربر
        /// </summary>
        public async Task<int> UpdateStreakAsync(int userId)
        {
            try
            {
                var metrics = await _context.Set<ProductivityMetrics>()
                    .FirstOrDefaultAsync(m => m.UserId == userId && m.IsCurrentPeriod);

                if (metrics == null)
                    return 0;

                var today = DateTime.UtcNow.Date;
                var lastActivity = metrics.LastActivityDate.Date;

                if (lastActivity == today)
                {
                    // Already counted for today
                    return metrics.CurrentStreak;
                }

                if (lastActivity.AddDays(1) == today)
                {
                    // Consecutive day - increment streak
                    metrics.CurrentStreak++;
                }
                else if (lastActivity < today.AddDays(-1))
                {
                    // Gap in activity - reset streak
                    metrics.CurrentStreak = 1;
                }

                metrics.LastActivityDate = today;

                if (metrics.CurrentStreak > metrics.LongestStreak)
                    metrics.LongestStreak = metrics.CurrentStreak;

                await _context.SaveChangesAsync();
                return metrics.CurrentStreak;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating streak for user {UserId}", userId);
                return 0;
            }
        }

        /// <summary>
        /// دریافت سطح بهره‌وری
        /// </summary>
        public async Task<ProductivityTier> GetProductivityTierAsync(int userId)
        {
            try
            {
                var score = await CalculateProductivityScoreAsync(userId);

                return score switch
                {
                    < 41 => ProductivityTier.Bronze,
                    < 61 => ProductivityTier.Silver,
                    < 81 => ProductivityTier.Gold,
                    < 95 => ProductivityTier.Platinum,
                    _ => ProductivityTier.Diamond
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting productivity tier for user {UserId}", userId);
                return ProductivityTier.Bronze;
            }
        }

        /// <summary>
        /// دریافت متریکس بهره‌وری کاربر
        /// </summary>
        public async Task<ProductivityMetricsDto> GetUserProductivityAsync(int userId)
        {
            try
            {
                var metrics = await _context.Set<ProductivityMetrics>()
                    .FirstOrDefaultAsync(m => m.UserId == userId && m.IsCurrentPeriod);

                if (metrics == null)
                    return new ProductivityMetricsDto();

                var tier = await GetProductivityTierAsync(userId);

                return new ProductivityMetricsDto
                {
                    UserId = userId,
                    ProductivityScore = metrics.ProductivityScore,
                    TaskCompletionRate = metrics.TaskCompletionRate,
                    OnTimeDeliveryRate = metrics.OnTimeDeliveryRate,
                    ConsistencyRate = metrics.ConsistencyRate,
                    QualityScore = metrics.QualityScore,
                    CurrentStreak = metrics.CurrentStreak,
                    LongestStreak = metrics.LongestStreak,
                    CurrentTier = (int)tier,
                    TotalTasksAssigned = metrics.TotalTasksAssigned,
                    TotalTasksCompleted = metrics.TotalTasksCompleted,
                    OnTimeTasksCompleted = metrics.OnTimeTasksCompleted
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user productivity for user {UserId}", userId);
                return new ProductivityMetricsDto();
            }
        }

        /// <summary>
        /// دریافت متریکس بهره‌وری تیم
        /// </summary>
        public async Task<TeamProductivityDto> GetTeamProductivityAsync(int teamId)
        {
            try
            {
                var teamMembers = await _context.Set<ProjectMember>()
                    .Where(pm => pm.ProjectId == teamId)
                    .Select(pm => pm.ApplicationUserId)
                    .Distinct()
                    .ToListAsync();

                if (teamMembers.Count == 0)
                    return new TeamProductivityDto();

                var metrics = await _context.Set<ProductivityMetrics>()
                    .Where(m => teamMembers.Contains(m.UserId) && m.IsCurrentPeriod)
                    .ToListAsync();

                return new TeamProductivityDto
                {
                    TeamId = teamId,
                    MemberCount = teamMembers.Count,
                    AverageProductivityScore = metrics.Any() ? Math.Round(metrics.Average(m => m.ProductivityScore), 2) : 0,
                    AverageTaskCompletionRate = metrics.Any() ? Math.Round(metrics.Average(m => m.TaskCompletionRate), 2) : 0,
                    AverageOnTimeDeliveryRate = metrics.Any() ? Math.Round(metrics.Average(m => m.OnTimeDeliveryRate), 2) : 0,
                    TotalTasksCompleted = metrics.Sum(m => m.TotalTasksCompleted),
                    AverageCurrentStreak = metrics.Any() ? Math.Round(metrics.Average(m => m.CurrentStreak), 2) : 0
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting team productivity for team {TeamId}", teamId);
                return new TeamProductivityDto();
            }
        }

        /// <summary>
        /// به‌روزرسانی متریکس بهره‌وری کاربر
        /// </summary>
        public async Task<bool> UpdateProductivityMetricsAsync(int userId, int workspaceId)
        {
            try
            {
                var existingMetrics = await _context.Set<ProductivityMetrics>()
                    .FirstOrDefaultAsync(m => m.UserId == userId && m.IsCurrentPeriod);

                var completionRate = await CalculateTaskCompletionRateAsync(userId);
                var onTimeRate = await CalculateOnTimeDeliveryRateAsync(userId);
                var consistencyRate = await CalculateConsistencyRateAsync(userId);
                var qualityScore = await CalculateQualityScoreAsync(userId);
                var productivityScore = await CalculateProductivityScoreAsync(userId);
                var tier = await GetProductivityTierAsync(userId);

                if (existingMetrics != null)
                {
                    existingMetrics.ProductivityScore = productivityScore;
                    existingMetrics.TaskCompletionRate = completionRate;
                    existingMetrics.OnTimeDeliveryRate = onTimeRate;
                    existingMetrics.ConsistencyRate = consistencyRate;
                    existingMetrics.QualityScore = qualityScore;
                    existingMetrics.CurrentTier = tier;
                    existingMetrics.ChangeDate = DateTime.UtcNow;
                    existingMetrics.ChangeUser = userId.ToString();

                    _context.Set<ProductivityMetrics>().Update(existingMetrics);
                }
                else
                {
                    var newMetrics = new ProductivityMetrics
                    {
                        UserId = userId,
                        WorkspaceId = workspaceId,
                        ProductivityScore = productivityScore,
                        TaskCompletionRate = completionRate,
                        OnTimeDeliveryRate = onTimeRate,
                        ConsistencyRate = consistencyRate,
                        QualityScore = qualityScore,
                        CurrentTier = tier,
                        PeriodStartDate = GetPeriodStart(),
                        PeriodEndDate = GetPeriodEnd(),
                        IsCurrentPeriod = true,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = userId.ToString()
                    };

                    _context.Set<ProductivityMetrics>().Add(newMetrics);
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating productivity metrics for user {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// دریافت تاریخچهٔ نمرات
        /// </summary>
        public async Task<List<ProductivityScoreDto>> GetScoreHistoryAsync(int userId, int days = 30)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-days);

                var history = await _context.Set<ProductivityScoreHistory>()
                    .Where(h => h.UserId == userId && h.SnapshotDate >= startDate)
                    .OrderByDescending(h => h.SnapshotDate)
                    .Select(h => new ProductivityScoreDto
                    {
                        SnapshotDate = h.SnapshotDate,
                        ProductivityScore = h.ProductivityScore,
                        TaskCompletionRate = h.TaskCompletionRate,
                        OnTimeDeliveryRate = h.OnTimeDeliveryRate,
                        ConsistencyRate = h.ConsistencyRate,
                        QualityScore = h.QualityScore,
                        CurrentStreak = h.CurrentStreak,
                        TierAtSnapshot = h.TierAtSnapshot
                    })
                    .ToListAsync();

                return history;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting score history for user {UserId}", userId);
                return new List<ProductivityScoreDto>();
            }
        }

        /// <summary>
        /// دریافت معیارهای تطابق
        /// </summary>
        public async Task<BenchmarkMetricsDto> GetBenchmarkMetricsAsync(int userId)
        {
            try
            {
                var userMetrics = await _context.Set<ProductivityMetrics>()
                    .FirstOrDefaultAsync(m => m.UserId == userId && m.IsCurrentPeriod);

                if (userMetrics == null)
                    return new BenchmarkMetricsDto();

                // Get workspace average
                var workspaceMetrics = await _context.Set<ProductivityMetrics>()
                    .Where(m => m.WorkspaceId == userMetrics.WorkspaceId && m.IsCurrentPeriod)
                    .ToListAsync();

                var workspaceAvg = workspaceMetrics.Any()
                    ? Math.Round(workspaceMetrics.Average(m => m.ProductivityScore), 2)
                    : 0;

                return new BenchmarkMetricsDto
                {
                    UserScore = userMetrics.ProductivityScore,
                    WorkspaceAverageScore = workspaceAvg,
                    UserRankPercentile = CalculatePercentile(userMetrics.ProductivityScore,
                        workspaceMetrics.Select(m => m.ProductivityScore).ToList()),
                    ComparisonToAverage = Math.Round(userMetrics.ProductivityScore - workspaceAvg, 2)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting benchmark metrics for user {UserId}", userId);
                return new BenchmarkMetricsDto();
            }
        }

        // Helper Methods
        private (DateTime start, DateTime end) GetDateRange(string period)
        {
            var now = DateTime.UtcNow;
            return period.ToLower() switch
            {
                "week" => (now.AddDays(-7), now),
                "month" => (now.AddDays(-30), now),
                "quarter" => (now.AddDays(-90), now),
                _ => (now.AddDays(-30), now)
            };
        }

        private DateTime GetPeriodStart()
        {
            var now = DateTime.UtcNow;
            return new DateTime(now.Year, now.Month, 1);
        }

        private DateTime GetPeriodEnd()
        {
            var now = DateTime.UtcNow;
            return now.AddMonths(1) < new DateTime(now.Year, now.Month, 1).AddMonths(1)
                ? now
                : new DateTime(now.Year, now.Month, 1).AddMonths(1).AddDays(-1);
        }

        private double CalculatePercentile(double value, List<double> allValues)
        {
            if (allValues.Count == 0)
                return 0;

            var sortedValues = allValues.OrderBy(v => v).ToList();
            var rank = sortedValues.Count(v => v <= value);
            return Math.Round((rank / (double)sortedValues.Count) * 100, 2);
        }
    }
}
