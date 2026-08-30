/*
| Module      : Gamification
| Class       : AbuseDetectionEngine
| Purpose     : موتور تشخیص سوء استفاده متقدم با قوانین متعدد
*/

using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace SmartTask.Web.Services.Gamification
{
    public class AbuseDetectionEngine : IAbuseDetectionEngine
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AbuseDetectionEngine> _logger;

        private const int RapidCompletionThreshold = 50; // Tasks per hour
        private const int SigmaThreshold = 5; // Standard deviations

        public AbuseDetectionEngine(ApplicationDbContext context, ILogger<AbuseDetectionEngine> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task ScanUserActivityAsync(int userId)
        {
            try
            {
                var detectionResults = new List<(AbuseReportType type, int severity, string evidence)>();

                // Rule 1: Rapid Completion Detection
                var rapidCompletionResult = await DetectRapidCompletionAsync(userId);
                if (rapidCompletionResult.isSuspicious)
                    detectionResults.Add((AbuseReportType.RapidCompletion, rapidCompletionResult.severity, rapidCompletionResult.evidence));

                // Rule 2: Velocity Anomaly Detection
                var velocityResult = await DetectVelocityAnomalyAsync(userId);
                if (velocityResult.isSuspicious)
                    detectionResults.Add((AbuseReportType.VelocityAnomaly, velocityResult.severity, velocityResult.evidence));

                // Rule 3: Duplicate Completion Detection
                var duplicateResult = await DetectDuplicateCompletionsAsync(userId);
                if (duplicateResult.isSuspicious)
                    detectionResults.Add((AbuseReportType.DuplicateCompletions, duplicateResult.severity, duplicateResult.evidence));

                // Rule 4: System Manipulation Detection
                var manipulationResult = await DetectSystemManipulationAsync(userId);
                if (manipulationResult.isSuspicious)
                    detectionResults.Add((AbuseReportType.SystemManipulation, manipulationResult.severity, manipulationResult.evidence));

                // Rule 5: Low-Estimate Task Farming
                var farmingResult = await DetectLowEstimateTaskFarmingAsync(userId);
                if (farmingResult.isSuspicious)
                    detectionResults.Add((AbuseReportType.LowEstimateTaskFarming, farmingResult.severity, farmingResult.evidence));

                // Create reports for flagged activities
                foreach (var result in detectionResults)
                {
                    var existingReport = await _context.Set<AbuseReport>()
                        .FirstOrDefaultAsync(r => r.UserId == userId &&
                                                  r.ReportType == result.type &&
                                                  r.Status == AbuseReportStatus.Pending);

                    if (existingReport != null)
                        continue; // Don't create duplicate reports

                    var report = new AbuseReport
                    {
                        UserId = userId,
                        ReportType = result.type,
                        Status = AbuseReportStatus.Pending,
                        Description = $"Auto-detected {result.type} activity",
                        Evidence = result.evidence,
                        SeverityScore = result.severity,
                        ConfidenceLevel = 0.85m,
                        AutoDetectionRule = result.type.ToString(),
                        DetectionDate = DateTime.UtcNow,
                        CreatedDate = DateTime.UtcNow,
                        CreatedBy = "AbuseDetectionEngine"
                    };

                    _context.Set<AbuseReport>().Add(report);
                }

                if (detectionResults.Count > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogWarning("Detected {Count} abuse patterns for user {UserId}", detectionResults.Count, userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scanning user activity for user {UserId}", userId);
            }
        }

        private async Task<(bool isSuspicious, int severity, string evidence)> DetectRapidCompletionAsync(int userId)
        {
            try
            {
                var oneHourAgo = DateTime.UtcNow.AddHours(-1);

                var recentCompletions = await _context.Set<WalletTransaction>()
                    .Where(t => t.UserProgressionId == (
                        _context.Set<UserProgression>()
                            .Where(up => up.UserId == userId)
                            .Select(up => up.Id)
                            .FirstOrDefault()
                    ) && t.TransactionDate >= oneHourAgo && t.TransactionType == TransactionType.Earned)
                    .CountAsync();

                if (recentCompletions > RapidCompletionThreshold)
                {
                    var evidence = new
                    {
                        TasksInLastHour = recentCompletions,
                        Threshold = RapidCompletionThreshold,
                        Excess = recentCompletions - RapidCompletionThreshold
                    };

                    return (true, Math.Min(100, recentCompletions * 2), JsonSerializer.Serialize(evidence));
                }

                return (false, 0, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting rapid completion for user {UserId}", userId);
                return (false, 0, "");
            }
        }

        private async Task<(bool isSuspicious, int severity, string evidence)> DetectVelocityAnomalyAsync(int userId)
        {
            try
            {
                var userProgression = await _context.Set<UserProgression>()
                    .FirstOrDefaultAsync(up => up.UserId == userId);

                if (userProgression == null)
                    return (false, 0, "");

                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

                var dailyXpGains = await _context.Set<WalletTransaction>()
                    .Where(t => t.UserProgressionId == userProgression.Id &&
                               t.TransactionDate >= thirtyDaysAgo &&
                               t.TransactionType == TransactionType.Earned)
                    .GroupBy(t => t.TransactionDate.Date)
                    .Select(g => (double)g.Sum(t => t.Amount))
                    .ToListAsync();

                if (dailyXpGains.Count < 3)
                    return (false, 0, "");

                var mean = dailyXpGains.Average();
                var stdDev = Math.Sqrt(dailyXpGains.Average(x => Math.Pow(x - mean, 2)));
                var today = dailyXpGains.Last();

                var zScore = stdDev > 0 ? (today - mean) / stdDev : 0;

                if (zScore > SigmaThreshold)
                {
                    var evidence = new
                    {
                        TodayXP = today,
                        AverageXP = mean,
                        StdDev = stdDev,
                        ZScore = zScore,
                        Threshold = SigmaThreshold
                    };

                    var severity = Math.Min(100, (int)(zScore * 10));
                    return (true, severity, JsonSerializer.Serialize(evidence));
                }

                return (false, 0, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting velocity anomaly for user {UserId}", userId);
                return (false, 0, "");
            }
        }

        private async Task<(bool isSuspicious, int severity, string evidence)> DetectDuplicateCompletionsAsync(int userId)
        {
            try
            {
                var twentyFourHoursAgo = DateTime.UtcNow.AddHours(-24);

                var completedTasks = await _context.Set<TaskItem>()
                    .Where(t => t.Assignments.Any(a => a.ApplicationUserId == userId) && t.CompletedDate >= twentyFourHoursAgo)
                    .GroupBy(t => t.Id)
                    .Select(g => new { TaskId = g.Key, CompletionCount = g.Count() })
                    .Where(x => x.CompletionCount > 1)
                    .ToListAsync();

                if (completedTasks.Count > 0)
                {
                    var evidence = new
                    {
                        DuplicateTasksFound = completedTasks.Count,
                        Details = completedTasks.Select(t => new { t.TaskId, t.CompletionCount })
                    };

                    var severity = Math.Min(100, completedTasks.Count * 20);
                    return (true, severity, JsonSerializer.Serialize(evidence));
                }

                return (false, 0, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting duplicate completions for user {UserId}", userId);
                return (false, 0, "");
            }
        }

        private async Task<(bool isSuspicious, int severity, string evidence)> DetectSystemManipulationAsync(int userId)
        {
            try
            {
                var sixHoursAgo = DateTime.UtcNow.AddHours(-6);

                var timestampMismatches = await _context.Set<TaskItem>()
                    .Where(t => t.Assignments.Any(a => a.ApplicationUserId == userId) &&
                               t.CompletedDate >= sixHoursAgo &&
                               (EF.Functions.DateDiffSecond(t.CreatedDate, t.CompletedDate) < 0))
                    .CountAsync();

                if (timestampMismatches > 0)
                {
                    var evidence = new
                    {
                        TimestampMismatches = timestampMismatches,
                        TimeWindow = "6 hours"
                    };

                    return (true, timestampMismatches * 25, JsonSerializer.Serialize(evidence));
                }

                return (false, 0, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting system manipulation for user {UserId}", userId);
                return (false, 0, "");
            }
        }

        private async Task<(bool isSuspicious, int severity, string evidence)> DetectLowEstimateTaskFarmingAsync(int userId)
        {
            try
            {
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

                var taskStats = await _context.Set<TaskItem>()
                    .Where(t => t.Assignments.Any(a => a.ApplicationUserId == userId) &&
                               t.CompletedDate >= thirtyDaysAgo &&
                               t.Estimate <= 1) // Tasks estimated for 1 hour or less
                    .GroupBy(t => 1)
                    .Select(g => new
                    {
                        LowEstimateTasks = g.Count(),
                        AverageHours = g.Average(t => t.Estimate)
                    })
                    .FirstOrDefaultAsync();

                if (taskStats != null && taskStats.LowEstimateTasks > 100)
                {
                    var evidence = new
                    {
                        LowEstimateTaskCount = taskStats.LowEstimateTasks,
                        AverageEstimate = taskStats.AverageHours,
                        Threshold = 100
                    };

                    var severity = Math.Min(100, (taskStats.LowEstimateTasks - 100) / 2);
                    return (true, severity, JsonSerializer.Serialize(evidence));
                }

                return (false, 0, "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting low-estimate task farming for user {UserId}", userId);
                return (false, 0, "");
            }
        }

        public async Task<List<dynamic>> GetPendingReportsAsync()
        {
            try
            {
                var reports = await _context.Set<AbuseReport>()
                    .Where(r => r.Status == AbuseReportStatus.Pending || r.Status == AbuseReportStatus.UnderReview)
                    .OrderByDescending(r => r.SeverityScore)
                    .Select(r => new
                    {
                        r.Id,
                        r.UserId,
                        UserName = r.User.UserName,
                        r.ReportType,
                        r.Status,
                        r.Description,
                        r.SeverityScore,
                        r.ConfidenceLevel,
                        r.DetectionDate,
                        r.AutoDetectionRule
                    })
                    .ToListAsync();

                return reports.Cast<dynamic>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pending reports");
                return new List<dynamic>();
            }
        }

        public async Task<dynamic> GetReportAsync(int reportId)
        {
            try
            {
                var report = await _context.Set<AbuseReport>()
                    .FirstOrDefaultAsync(r => r.Id == reportId);

                if (report == null)
                    return null;

                return new
                {
                    report.Id,
                    report.UserId,
                    UserName = report.User.UserName,
                    report.ReportType,
                    report.Status,
                    report.Description,
                    report.Evidence,
                    report.SeverityScore,
                    report.ConfidenceLevel,
                    report.DetectionDate,
                    report.ReviewedDate,
                    ReviewedByUserName = report.ReviewedByUser?.UserName,
                    report.ReviewNotes,
                    report.RewardsRefunded,
                    report.RewardsSuspended,
                    report.RefundedAmount,
                    report.SuspensionUntil
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report {ReportId}", reportId);
                return null;
            }
        }

        public async Task ResolveReportAsync(int reportId, AbuseReportStatus status, string notes, int? reviewedByUserId = null)
        {
            try
            {
                var report = await _context.Set<AbuseReport>()
                    .FirstOrDefaultAsync(r => r.Id == reportId);

                if (report == null)
                    return;

                report.Status = status;
                report.ReviewNotes = notes;
                report.ReviewedDate = DateTime.UtcNow;
                report.ReviewedByUserId = reviewedByUserId;
                report.ChangeDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Resolved abuse report {ReportId} with status {Status}", reportId, status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving report {ReportId}", reportId);
            }
        }

        public async Task RefundRewardAsync(int reportId, int amount)
        {
            try
            {
                var report = await _context.Set<AbuseReport>()
                    .FirstOrDefaultAsync(r => r.Id == reportId);

                if (report == null || report.RewardsRefunded)
                    return;

                var wallet = await _context.Set<UserWallet>()
                    .FirstOrDefaultAsync(w => w.UserId == report.UserId);

                if (wallet == null)
                    return;

                wallet.AvailablePoints -= amount;
                wallet.TotalPoints -= amount;

                var transaction = new WalletTransaction
                {
                    UserWalletId = wallet.Id,
                    Amount = amount,
                    TransactionType = TransactionType.Refund,
                    Description = $"Abuse refund for report {reportId}",
                    TransactionDate = DateTime.UtcNow,
                    CreatedBy = "AbuseDetectionEngine",
                    CreatedDate = DateTime.UtcNow
                };

                _context.Set<WalletTransaction>().Add(transaction);
                report.RewardsRefunded = true;
                report.RefundedAmount = amount;

                await _context.SaveChangesAsync();
                _logger.LogWarning("Refunded {Amount} points for user {UserId} (report {ReportId})",
                    amount, report.UserId, reportId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refunding reward for report {ReportId}", reportId);
            }
        }

        public async Task SuspendRewardsAsync(int reportId, DateTime until)
        {
            try
            {
                var report = await _context.Set<AbuseReport>()
                    .FirstOrDefaultAsync(r => r.Id == reportId);

                if (report == null)
                    return;

                report.RewardsSuspended = true;
                report.SuspensionUntil = until;
                await _context.SaveChangesAsync();

                _logger.LogWarning("Suspended rewards for user {UserId} until {Until}",
                    report.UserId, until);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error suspending rewards for report {ReportId}", reportId);
            }
        }

        public async Task ResumeRewardsAsync(int userId)
        {
            try
            {
                var suspensions = await _context.Set<AbuseReport>()
                    .Where(r => r.UserId == userId && r.RewardsSuspended && r.SuspensionUntil <= DateTime.UtcNow)
                    .ToListAsync();

                foreach (var report in suspensions)
                {
                    report.RewardsSuspended = false;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Resumed rewards for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming rewards for user {UserId}", userId);
            }
        }

        public async Task<bool> IsUserSuspendedAsync(int userId)
        {
            try
            {
                var suspension = await _context.Set<AbuseReport>()
                    .FirstOrDefaultAsync(r => r.UserId == userId &&
                                              r.RewardsSuspended &&
                                              (r.SuspensionUntil == null || r.SuspensionUntil > DateTime.UtcNow));

                return suspension != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking suspension status for user {UserId}", userId);
                return false;
            }
        }
    }
}
