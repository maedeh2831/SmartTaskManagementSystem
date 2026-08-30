/*
| Module      : Gamification
| Class       : GamificationAnalyticsService
| Purpose     : خدمات تحلیلی جامع برای گیمیفیکیشن
*/

using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.ViewModels.Gamification.Admin;
using Microsoft.EntityFrameworkCore;

namespace SmartTask.Web.Services.Gamification
{
    public class GamificationAnalyticsService : IGamificationAnalyticsService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<GamificationAnalyticsService> _logger;

        public GamificationAnalyticsService(ApplicationDbContext context, ILogger<GamificationAnalyticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<EconomyMetricsDto> GetEconomyMetricsAsync()
        {
            try
            {
                var oneWeekAgo = DateTime.UtcNow.AddDays(-7);
                var oneMonthAgo = DateTime.UtcNow.AddDays(-30);

                var totalXp = await _context.Set<UserProgression>()
                    .SumAsync(up => up.TotalExperience);

                var totalMomentum = await _context.Set<UserWallet>()
                    .SumAsync(uw => uw.TotalPoints);

                var userCount = await _context.Set<UserProgression>()
                    .CountAsync();

                var activeLastWeek = await _context.Set<ActivityLog>()
                    .Where(al => al.CreatedDate >= oneWeekAgo)
                    .Select(al => al.CreatedBy)
                    .Distinct()
                    .CountAsync();

                var activeLastMonth = await _context.Set<ActivityLog>()
                    .Where(al => al.CreatedDate >= oneMonthAgo)
                    .Select(al => al.CreatedBy)
                    .Distinct()
                    .CountAsync();

                var achievementsUnlocked = await _context.Set<UserAchievement>()
                    .CountAsync();

                var marketplaceTransactionsWeek = await _context.Set<MarketplaceTransaction>()
                    .Where(mt => mt.TransactionDate >= oneWeekAgo)
                    .CountAsync();

                var marketplaceTransactionsMonth = await _context.Set<MarketplaceTransaction>()
                    .Where(mt => mt.TransactionDate >= oneMonthAgo)
                    .CountAsync();

                return new EconomyMetricsDto
                {
                    TotalXpDistributed = totalXp,
                    TotalMomentumCirculating = totalMomentum,
                    AverageMomentumPerUser = userCount > 0 ? (decimal)totalMomentum / userCount : 0,
                    ActiveUsersInLastWeek = activeLastWeek,
                    ActiveUsersInLastMonth = activeLastMonth,
                    AverageXpPerActiveUser = activeLastMonth > 0 ? (decimal)totalXp / activeLastMonth : 0,
                    AchievementUnlockRate = userCount > 0 ? ((decimal)achievementsUnlocked / userCount) * 100 : 0,
                    TotalAchievementsUnlocked = achievementsUnlocked,
                    MarketplaceTransactionsLastWeek = marketplaceTransactionsWeek,
                    MarketplaceTransactionsLastMonth = marketplaceTransactionsMonth,
                    PurchaseVelocity = marketplaceTransactionsMonth / 30m,
                    CalculatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating economy metrics");
                return new EconomyMetricsDto { CalculatedAt = DateTime.UtcNow };
            }
        }

        public async Task<List<dynamic>> GetDailyActiveUsersAsync(int days = 30)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-days);

                var dailyActive = await _context.Set<ActivityLog>()
                    .Where(al => al.CreatedDate >= startDate)
                    .GroupBy(al => al.CreatedDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        ActiveUsers = g.Select(al => al.CreatedBy).Distinct().Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                return dailyActive.Cast<dynamic>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting daily active users");
                return new List<dynamic>();
            }
        }

        public async Task<List<dynamic>> GetAverageXpPerUserAsync(int days = 30)
        {
            try
            {
                var startDate = DateTime.UtcNow.AddDays(-days);

                var avgXpPerDay = await _context.Set<WalletTransaction>()
                    .Where(wt => wt.TransactionDate >= startDate && wt.TransactionType == Models.Enums.TransactionType.Earned)
                    .GroupBy(wt => wt.TransactionDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        TotalXp = g.Sum(wt => wt.Amount),
                        UniqueUsers = g.Select(wt => wt.UserWallet.UserId).Distinct().Count()
                    })
                    .OrderBy(x => x.Date)
                    .ToListAsync();

                var result = avgXpPerDay.Select(x => new
                {
                    x.Date,
                    x.TotalXp,
                    x.UniqueUsers,
                    AverageXpPerUser = x.UniqueUsers > 0 ? x.TotalXp / x.UniqueUsers : 0
                }).Cast<dynamic>().ToList();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting average XP per user");
                return new List<dynamic>();
            }
        }

        public async Task<List<dynamic>> GetAchievementUnlockRatesAsync()
        {
            try
            {
                var achievements = await _context.Set<Achievement>()
                    .Select(a => new
                    {
                        AchievementId = a.Id,
                        AchievementName = a.Name,
                        Category = a.Category,
                        UnlockCount = a.UserAchievements.Count(),
                        UnlockPercentage = _context.Set<UserProgression>().Count() > 0
                            ? ((decimal)a.UserAchievements.Count() / _context.Set<UserProgression>().Count()) * 100
                            : 0
                    })
                    .OrderByDescending(x => x.UnlockCount)
                    .ToListAsync();

                return achievements.Cast<dynamic>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting achievement unlock rates");
                return new List<dynamic>();
            }
        }

        public async Task<List<dynamic>> GetLevelDistributionAsync()
        {
            try
            {
                var distribution = await _context.Set<Leaderboard>()
                    .GroupBy(l => l.CurrentLevel)
                    .Select(g => new
                    {
                        Level = g.Key,
                        UserCount = g.Count(),
                        Percentage = _context.Set<Leaderboard>().Count() > 0
                            ? ((decimal)g.Count() / _context.Set<Leaderboard>().Count()) * 100
                            : 0
                    })
                    .OrderBy(x => x.Level)
                    .ToListAsync();

                return distribution.Cast<dynamic>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting level distribution");
                return new List<dynamic>();
            }
        }

        public async Task<UserProgressionAdminDto> GetUserProgressionAdminAsync(int userId)
        {
            try
            {
                var progression = await _context.Set<UserProgression>()
                    .FirstOrDefaultAsync(up => up.UserId == userId);

                var wallet = await _context.Set<UserWallet>()
                    .FirstOrDefaultAsync(uw => uw.UserId == userId);

                var leaderboard = await _context.Set<Leaderboard>()
                    .FirstOrDefaultAsync(l => l.UserId == userId);

                var streak = await _context.Set<UserStreak>()
                    .FirstOrDefaultAsync(us => us.UserId == userId);

                var abuseReports = await _context.Set<AbuseReport>()
                    .Where(ar => ar.UserId == userId)
                    .ToListAsync();

                var user = await _context.Users.FindAsync(userId);

                return new UserProgressionAdminDto
                {
                    UserId = userId,
                    UserName = user?.UserName ?? "Unknown",
                    Level = progression?.CurrentLevel ?? 1,
                    TotalExperience = progression?.TotalExperience ?? 0,
                    TotalPoints = wallet?.TotalPoints ?? 0,
                    AvailablePoints = wallet?.AvailablePoints ?? 0,
                    TasksCompleted = progression?.TasksCompleted ?? 0,
                    ProjectsCompleted = progression?.ProjectsCompleted ?? 0,
                    AchievementsUnlocked = progression?.Achievements.Count ?? 0,
                    CurrentStreak = streak?.CurrentStreak ?? 0,
                    LongestStreak = streak?.LongestStreak ?? 0,
                    GlobalRank = leaderboard?.GlobalRank ?? 0,
                    RewardsSuspended = abuseReports.Any(ar => ar.RewardsSuspended && (ar.SuspensionUntil == null || ar.SuspensionUntil > DateTime.UtcNow)),
                    SuspensionUntil = abuseReports.Where(ar => ar.RewardsSuspended).MaxBy(ar => ar.SuspensionUntil)?.SuspensionUntil,
                    LastActivityDate = progression?.ChangeDate ?? DateTime.UtcNow,
                    AbuseReportsCount = abuseReports.Count,
                    ConfirmedAbuseReports = abuseReports.Count(ar => ar.Status == Models.Enums.AbuseReportStatus.Confirmed)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user progression for user {UserId}", userId);
                return null;
            }
        }

        public async Task<List<UserProgressionAdminDto>> GetTopUsersAsync(int limit = 20)
        {
            try
            {
                var topUsers = await _context.Set<Leaderboard>()
                    .OrderBy(l => l.GlobalRank)
                    .Take(limit)
                    .Select(l => l.UserId)
                    .ToListAsync();

                var results = new List<UserProgressionAdminDto>();

                foreach (var userId in topUsers)
                {
                    var dto = await GetUserProgressionAdminAsync(userId);
                    if (dto != null)
                        results.Add(dto);
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting top users");
                return new List<UserProgressionAdminDto>();
            }
        }

        public async Task<List<dynamic>> GetMarketplaceMetricsAsync()
        {
            try
            {
                var metrics = await _context.Set<MarketplaceItem>()
                    .Select(mi => new
                    {
                        ItemId = mi.Id,
                        ItemName = mi.Name,
                        Category = mi.Category,
                        Price = mi.Price,
                        PurchaseCount = mi.Transactions.Count(),
                        Revenue = mi.Transactions.Sum(mt => mt.PointsSpent),
                        IsActive = mi.IsActive
                    })
                    .OrderByDescending(x => x.PurchaseCount)
                    .ToListAsync();

                return metrics.Cast<dynamic>().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting marketplace metrics");
                return new List<dynamic>();
            }
        }
    }
}
