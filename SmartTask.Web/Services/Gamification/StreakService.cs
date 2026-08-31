/*
| Module      : Gamification
| Class       : StreakService
| Purpose     : پیاده‌سازی خدمات رشته‌های بهره‌وری روزانه
*/

using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace SmartTask.Web.Services.Gamification
{
    public class StreakService : IStreakService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StreakService> _logger;

        private readonly Dictionary<int, int> _milestoneBonuses = new()
        {
            { 3, 150 },
            { 7, 300 },
            { 14, 500 },
            { 30, 1000 },
            { 100, 5000 }
        };

        public StreakService(ApplicationDbContext context, ILogger<StreakService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> GetCurrentStreakAsync(int userId)
        {
            try
            {
                var streak = await _context.Set<UserStreak>()
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (streak == null)
                    return 0;

                // Check if streak is broken
                var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(streak.UserTimeZone ?? "UTC");
                var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, userTimeZone);
                var lastCompletionInUserTz = TimeZoneInfo.ConvertTime(streak.LastCompletionDate, userTimeZone);

                var daysSinceLastCompletion = (now.Date - lastCompletionInUserTz.Date).TotalDays;

                if (daysSinceLastCompletion > 1)
                {
                    // Streak is broken
                    streak.CurrentStreak = 0;
                    await _context.SaveChangesAsync();
                    return 0;
                }

                return streak.CurrentStreak;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current streak for user {UserId}", userId);
                return 0;
            }
        }

        public async Task<int> GetLongestStreakAsync(int userId)
        {
            try
            {
                var streak = await _context.Set<UserStreak>()
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                return streak?.LongestStreak ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting longest streak for user {UserId}", userId);
                return 0;
            }
        }

        public async Task UpdateStreakAsync(int userId, int xpGained)
        {
            try
            {
                var streak = await _context.Set<UserStreak>()
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (streak == null)
                {
                    // Create new streak
                    streak = new UserStreak
                    {
                        UserId = userId,
                        CurrentStreak = 1,
                        LongestStreak = 1,
                        StreakStartDate = DateTime.UtcNow,
                        LastCompletionDate = DateTime.UtcNow,
                        TasksCompletedToday = 1,
                        XpGainedToday = xpGained,
                        UserTimeZone = "UTC"
                    };
                    _context.Set<UserStreak>().Add(streak);
                }
                else
                {
                    var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(streak.UserTimeZone ?? "UTC");
                    var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, userTimeZone);
                    var lastCompletionInUserTz = TimeZoneInfo.ConvertTime(streak.LastCompletionDate, userTimeZone);

                    // Same day completion
                    if (now.Date == lastCompletionInUserTz.Date)
                    {
                        streak.TasksCompletedToday++;
                        streak.XpGainedToday += xpGained;
                    }
                    else if ((now.Date - lastCompletionInUserTz.Date).TotalDays == 1)
                    {
                        // Consecutive day - continue streak
                        streak.CurrentStreak++;
                        streak.TasksCompletedToday = 1;
                        streak.XpGainedToday = xpGained;

                        if (streak.CurrentStreak > streak.LongestStreak)
                        {
                            streak.LongestStreak = streak.CurrentStreak;
                        }
                    }
                    else
                    {
                        // Streak broken
                        streak.CurrentStreak = 1;
                        streak.StreakStartDate = DateTime.UtcNow;
                        streak.TasksCompletedToday = 1;
                        streak.XpGainedToday = xpGained;
                    }

                    streak.LastCompletionDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated streak for user {UserId}: Current={Current}", userId, streak.CurrentStreak);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating streak for user {UserId}", userId);
            }
        }

        public async Task ResetStreaksAsync()
        {
            try
            {
                var allStreaks = await _context.Set<UserStreak>().ToListAsync();

                foreach (var streak in allStreaks)
                {
                    var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(streak.UserTimeZone ?? "UTC");
                    var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, userTimeZone);
                    var lastResetInUserTz = TimeZoneInfo.ConvertTime(streak.LastResetDate, userTimeZone);

                    if (now.Date > lastResetInUserTz.Date)
                    {
                        streak.TasksCompletedToday = 0;
                        streak.XpGainedToday = 0;
                        streak.LastResetDate = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Daily streak reset completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting streaks");
            }
        }

        public async Task<(int current, int longest, int milestonesReached)> CheckMilestonesAsync(int userId)
        {
            try
            {
                var streak = await _context.Set<UserStreak>()
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (streak == null)
                    return (0, 0, 0);

                int milestonesReached = 0;
                var bonusPoints = 0;

                if (streak.CurrentStreak >= 3 && !streak.Milestone3Days)
                {
                    streak.Milestone3Days = true;
                    milestonesReached++;
                    bonusPoints += _milestoneBonuses[3];
                }

                if (streak.CurrentStreak >= 7 && !streak.Milestone7Days)
                {
                    streak.Milestone7Days = true;
                    milestonesReached++;
                    bonusPoints += _milestoneBonuses[7];
                }

                if (streak.CurrentStreak >= 14 && !streak.Milestone14Days)
                {
                    streak.Milestone14Days = true;
                    milestonesReached++;
                    bonusPoints += _milestoneBonuses[14];
                }

                if (streak.CurrentStreak >= 30 && !streak.Milestone30Days)
                {
                    streak.Milestone30Days = true;
                    milestonesReached++;
                    bonusPoints += _milestoneBonuses[30];
                }

                if (streak.CurrentStreak >= 100 && !streak.Milestone100Days)
                {
                    streak.Milestone100Days = true;
                    milestonesReached++;
                    bonusPoints += _milestoneBonuses[100];
                }

                if (milestonesReached > 0)
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("User {UserId} reached {Milestones} milestones, bonus: {Bonus} points",
                        userId, milestonesReached, bonusPoints);
                }

                return (streak.CurrentStreak, streak.LongestStreak, milestonesReached);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking milestones for user {UserId}", userId);
                return (0, 0, 0);
            }
        }

        public async Task<DateTime> GetNextResetTimeAsync(int userId)
        {
            try
            {
                var streak = await _context.Set<UserStreak>()
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (streak == null)
                    return DateTime.UtcNow.AddDays(1);

                var userTimeZone = TimeZoneInfo.FindSystemTimeZoneById(streak.UserTimeZone ?? "UTC");
                var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, userTimeZone);
                var nextReset = now.Date.AddDays(1);

                return TimeZoneInfo.ConvertTimeToUtc(nextReset, userTimeZone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting next reset time for user {UserId}", userId);
                return DateTime.UtcNow.AddDays(1);
            }
        }

        public async Task SetUserTimeZoneAsync(int userId, string timeZone)
        {
            try
            {
                var streak = await _context.Set<UserStreak>()
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (streak == null)
                {
                    streak = new UserStreak
                    {
                        UserId = userId,
                        UserTimeZone = timeZone
                    };
                    _context.Set<UserStreak>().Add(streak);
                }
                else
                {
                    streak.UserTimeZone = timeZone;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Set timezone for user {UserId} to {TimeZone}", userId, timeZone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting timezone for user {UserId}", userId);
            }
        }
    }
}
