/*
| Module      : Infrastructure
| Class       : GamificationBackgroundService
| Purpose     : خدمات پس‌زمینه برای محاسبه رتبه‌بندی و بروزرسانی‌های تناوبی
*/

using SmartTask.Web.Services.Gamification;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace SmartTask.Web.Infrastructure.BackgroundJobs
{
    public class GamificationBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GamificationBackgroundService> _logger;
        private Timer? _leaderboardRecalculationTimer;
        private Timer? _streakResetTimer;
        private Timer? _seasonalEventTimer;
        private Timer? _abuseDetectionTimer;
        private Timer? _productivityMetricsTimer;
        private readonly TimeSpan _leaderboardRecalculationInterval = TimeSpan.FromHours(1);
        private readonly TimeSpan _streakResetInterval = TimeSpan.FromHours(1); // Check hourly, reset at midnight
        private readonly TimeSpan _seasonalEventInterval = TimeSpan.FromHours(6); // Check every 6 hours
        private readonly TimeSpan _abuseDetectionInterval = TimeSpan.FromHours(1); // Hourly abuse scans
        private readonly TimeSpan _productivityMetricsInterval = TimeSpan.FromHours(1); // Recalculate hourly, store snapshots daily

        public GamificationBackgroundService(IServiceProvider serviceProvider, ILogger<GamificationBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Gamification Background Service is starting - Phase 5 Advanced Features");

            // Schedule leaderboard recalculation to run hourly
            _leaderboardRecalculationTimer = new Timer(
                callback: async _ => await RecalculateLeaderboardsAsync(),
                state: null,
                dueTime: TimeSpan.FromMinutes(5),
                period: _leaderboardRecalculationInterval
            );

            // Schedule streak reset (daily at midnight per timezone)
            _streakResetTimer = new Timer(
                callback: async _ => await ResetStreaksAsync(),
                state: null,
                dueTime: TimeSpan.FromMinutes(10),
                period: _streakResetInterval
            );

            // Schedule seasonal event processing
            _seasonalEventTimer = new Timer(
                callback: async _ => await ProcessSeasonalEventsAsync(),
                state: null,
                dueTime: TimeSpan.FromMinutes(15),
                period: _seasonalEventInterval
            );

            // Schedule abuse detection scans
            _abuseDetectionTimer = new Timer(
                callback: async _ => await RunAbuseDetectionAsync(),
                state: null,
                dueTime: TimeSpan.FromMinutes(20),
                period: _abuseDetectionInterval
            );

            // Schedule productivity metrics recalculation
            _productivityMetricsTimer = new Timer(
                callback: async _ => await RecalculateProductivityMetricsAsync(),
                state: null,
                dueTime: TimeSpan.FromMinutes(25),
                period: _productivityMetricsInterval
            );

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _leaderboardRecalculationTimer?.Dispose();
            _streakResetTimer?.Dispose();
            _seasonalEventTimer?.Dispose();
            _abuseDetectionTimer?.Dispose();
            _productivityMetricsTimer?.Dispose();
            _logger.LogInformation("Gamification Background Service is stopping");
        }

        private async Task RecalculateLeaderboardsAsync()
        {
            try
            {
                _logger.LogInformation("Starting scheduled leaderboard recalculation");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var leaderboardService = scope.ServiceProvider.GetRequiredService<ILeaderboardService>();
                    await leaderboardService.RecalculateAllLeaderboardsAsync();

                    _logger.LogInformation("Scheduled leaderboard recalculation completed successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scheduled leaderboard recalculation");
            }
        }

        private async Task ResetStreaksAsync()
        {
            try
            {
                _logger.LogInformation("Starting scheduled streak reset check");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var streakService = scope.ServiceProvider.GetRequiredService<IStreakService>();
                    await streakService.ResetStreaksAsync();

                    _logger.LogInformation("Scheduled streak reset check completed successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scheduled streak reset");
            }
        }

        private async Task ProcessSeasonalEventsAsync()
        {
            try
            {
                _logger.LogInformation("Starting scheduled seasonal event processing");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var seasonalService = scope.ServiceProvider.GetRequiredService<ISeasonalEventService>();
                    await seasonalService.UpdateEventStatusesAsync();

                    _logger.LogInformation("Scheduled seasonal event processing completed successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scheduled seasonal event processing");
            }
        }

        private async Task RunAbuseDetectionAsync()
        {
            try
            {
                _logger.LogInformation("Starting scheduled abuse detection scan");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var abuseEngine = scope.ServiceProvider.GetRequiredService<IAbuseDetectionEngine>();
                    var context = scope.ServiceProvider.GetRequiredService<SmartTask.Web.Data.Context.ApplicationDbContext>();

                    // Get all active users
                    var activeUsers = context.Set<SmartTask.Web.Models.Entities.ActivityLog>()
                        .Where(al => al.CreatedDate >= DateTime.UtcNow.AddHours(-24))
                        .Select(al => al.CreatedBy)
                        .Distinct()
                        .ToList();

                    foreach (var userId in activeUsers)
                    {
                        if (int.TryParse(userId, out var parsedUserId))
                        {
                            await abuseEngine.ScanUserActivityAsync(parsedUserId);
                        }
                    }

                    _logger.LogInformation("Scheduled abuse detection scan completed for {UserCount} users", activeUsers.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scheduled abuse detection");
            }
        }

        private async Task RecalculateProductivityMetricsAsync()
        {
            try
            {
                _logger.LogInformation("Starting scheduled productivity metrics recalculation");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var productivityService = scope.ServiceProvider.GetRequiredService<IProductivityMetricsService>();
                    var context = scope.ServiceProvider.GetRequiredService<SmartTask.Web.Data.Context.ApplicationDbContext>();

                    // Get all active users in the past 24 hours
                    var activeUsers = await context.Set<SmartTask.Web.Models.Entities.ActivityLog>()
                        .Where(al => al.CreatedDate >= DateTime.UtcNow.AddHours(-24))
                        .Select(al => al.ApplicationUserId)
                        .Distinct()
                        .ToListAsync();

                    int updatedCount = 0;

                    foreach (var userId in activeUsers)
                    {
                        try
                        {
                            // Get user's workspace(s) - update for primary workspace
                            var userWorkspaces = await context.Set<SmartTask.Web.Models.Entities.WorkspaceMember>()
                                .Where(wm => wm.ApplicationUserId == userId)
                                .Select(wm => wm.WorkspaceId)
                                .FirstOrDefaultAsync();

                            if (userWorkspaces > 0)
                            {
                                var success = await productivityService.UpdateProductivityMetricsAsync(userId, userWorkspaces);
                                if (success)
                                {
                                    // Create daily snapshot
                                    var metrics = await context.Set<SmartTask.Web.Models.Entities.ProductivityMetrics>()
                                        .FirstOrDefaultAsync(m => m.UserId == userId && m.IsCurrentPeriod);

                                    if (metrics != null)
                                    {
                                        var snapshot = new SmartTask.Web.Models.Entities.ProductivityScoreHistory
                                        {
                                            ProductivityMetricsId = metrics.Id,
                                            UserId = userId,
                                            ProductivityScore = metrics.ProductivityScore,
                                            TaskCompletionRate = metrics.TaskCompletionRate,
                                            OnTimeDeliveryRate = metrics.OnTimeDeliveryRate,
                                            ConsistencyRate = metrics.ConsistencyRate,
                                            QualityScore = metrics.QualityScore,
                                            TasksCompletedThisPeriod = metrics.TotalTasksCompleted,
                                            OnTimeTasksThisPeriod = metrics.OnTimeTasksCompleted,
                                            CurrentStreak = metrics.CurrentStreak,
                                            SnapshotDate = DateTime.UtcNow,
                                            PeriodType = "Daily",
                                            TierAtSnapshot = (int)metrics.CurrentTier,
                                            CreatedDate = DateTime.UtcNow,
                                            CreatedBy = "System"
                                        };

                                        context.Set<SmartTask.Web.Models.Entities.ProductivityScoreHistory>().Add(snapshot);
                                        await context.SaveChangesAsync();
                                        updatedCount++;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error updating productivity metrics for user {UserId}", userId);
                        }
                    }

                    _logger.LogInformation("Scheduled productivity metrics recalculation completed for {UserCount} users", updatedCount);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scheduled productivity metrics recalculation");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _leaderboardRecalculationTimer?.Dispose();
            _streakResetTimer?.Dispose();
            _seasonalEventTimer?.Dispose();
            _abuseDetectionTimer?.Dispose();
            _productivityMetricsTimer?.Dispose();
            await base.StopAsync(cancellationToken);
        }
    }
}
