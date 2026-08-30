/*
| Module      : Gamification
| Class       : LeaderboardInitializationService
| Purpose     : خدمات پس‌زمینه برای مقداردهی و بروزرسانی ورودی‌های رتبه‌بندی
*/

using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace SmartTask.Web.Services.Gamification
{
    public class LeaderboardInitializationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LeaderboardInitializationService> _logger;

        public LeaderboardInitializationService(ApplicationDbContext context, ILogger<LeaderboardInitializationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Initialize leaderboard entry for a new user
        /// </summary>
        public async Task InitializeUserLeaderboardAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found", userId);
                    return;
                }

                // Check if global leaderboard entry exists
                var globalEntry = await _context.Set<Leaderboard>()
                    .FirstOrDefaultAsync(l => l.UserId == userId && l.WorkspaceId == null);

                if (globalEntry == null)
                {
                    globalEntry = new Leaderboard
                    {
                        UserId = userId,
                        User = user,
                        WorkspaceId = null,
                        GlobalRank = int.MaxValue, // Will be recalculated
                        WorkspaceRank = int.MaxValue,
                        CurrentLevel = 1,
                        TotalPoints = 0,
                        TotalExperience = 0,
                        LastUpdated = DateTime.UtcNow,
                        CalculatedAt = DateTime.UtcNow
                    };

                    _context.Set<Leaderboard>().Add(globalEntry);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Initialized leaderboard entry for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing leaderboard for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Initialize workspace leaderboard entries for a user
        /// </summary>
        public async Task InitializeWorkspaceLeaderboardAsync(int userId, int workspaceId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found", userId);
                    return;
                }

                var workspaceEntry = await _context.Set<Leaderboard>()
                    .FirstOrDefaultAsync(l => l.UserId == userId && l.WorkspaceId == workspaceId);

                if (workspaceEntry == null)
                {
                    workspaceEntry = new Leaderboard
                    {
                        UserId = userId,
                        User = user,
                        WorkspaceId = workspaceId,
                        GlobalRank = int.MaxValue,
                        WorkspaceRank = int.MaxValue,
                        CurrentLevel = 1,
                        TotalPoints = 0,
                        TotalExperience = 0,
                        LastUpdated = DateTime.UtcNow,
                        CalculatedAt = DateTime.UtcNow
                    };

                    _context.Set<Leaderboard>().Add(workspaceEntry);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Initialized workspace leaderboard entry for user {UserId} in workspace {WorkspaceId}", userId, workspaceId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing workspace leaderboard for user {UserId} in workspace {WorkspaceId}", userId, workspaceId);
            }
        }

        /// <summary>
        /// Initialize team leaderboard for a team
        /// </summary>
        public async Task InitializeTeamLeaderboardAsync(int teamId, int workspaceId)
        {
            try
            {
                var team = await _context.Set<Team>().FindAsync(teamId);
                if (team == null)
                {
                    _logger.LogWarning("Team {TeamId} not found", teamId);
                    return;
                }

                var teamEntry = await _context.Set<TeamLeaderboard>()
                    .FirstOrDefaultAsync(tl => tl.TeamId == teamId && tl.WorkspaceId == workspaceId);

                if (teamEntry == null)
                {
                    teamEntry = new TeamLeaderboard
                    {
                        TeamId = teamId,
                        WorkspaceId = workspaceId,
                        Team = team,
                        TeamRank = int.MaxValue,
                        TotalTeamPoints = 0,
                        AverageTeamLevel = 1,
                        TotalTeamExperience = 0,
                        LastUpdated = DateTime.UtcNow,
                        CalculatedAt = DateTime.UtcNow
                    };

                    _context.Set<TeamLeaderboard>().Add(teamEntry);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Initialized team leaderboard for team {TeamId} in workspace {WorkspaceId}", teamId, workspaceId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing team leaderboard for team {TeamId}", teamId);
            }
        }

        /// <summary>
        /// Update user's leaderboard stats based on progression
        /// </summary>
        public async Task UpdateUserLeaderboardStatsAsync(int userId)
        {
            try
            {
                var progression = await _context.Set<UserProgression>()
                    .FirstOrDefaultAsync(up => up.UserId == userId);

                if (progression == null)
                {
                    return;
                }

                // Update global entry
                var globalEntry = await _context.Set<Leaderboard>()
                    .FirstOrDefaultAsync(l => l.UserId == userId && l.WorkspaceId == null);

                if (globalEntry != null)
                {
                    globalEntry.CurrentLevel = progression.CurrentLevel;
                    globalEntry.TotalExperience = progression.TotalExperience;
                    globalEntry.TasksCompleted = progression.TasksCompleted;
                    globalEntry.ProjectsCompleted = progression.ProjectsCompleted;
                    globalEntry.LastUpdated = DateTime.UtcNow;
                }

                // Get user's achievements count
                var achievementCount = await _context.Set<UserAchievement>()
                    .Where(ua => ua.UserId == userId && ua.UnlockedDate != null)
                    .CountAsync();

                if (globalEntry != null)
                {
                    globalEntry.AchievementsUnlocked = achievementCount;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated leaderboard stats for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating leaderboard stats for user {UserId}", userId);
            }
        }

        /// <summary>
        /// Get or create leaderboard entry for a user in a workspace
        /// </summary>
        public async Task<Leaderboard> GetOrCreateLeaderboardEntryAsync(int userId, int? workspaceId = null)
        {
            try
            {
                var entry = await _context.Set<Leaderboard>()
                    .FirstOrDefaultAsync(l => l.UserId == userId &&
                                             (workspaceId == null ? l.WorkspaceId == null : l.WorkspaceId == workspaceId));

                if (entry == null)
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null)
                    {
                        throw new InvalidOperationException($"User {userId} not found");
                    }

                    entry = new Leaderboard
                    {
                        UserId = userId,
                        User = user,
                        WorkspaceId = workspaceId,
                        GlobalRank = int.MaxValue,
                        WorkspaceRank = int.MaxValue,
                        CurrentLevel = 1,
                        TotalPoints = 0,
                        TotalExperience = 0,
                        LastUpdated = DateTime.UtcNow,
                        CalculatedAt = DateTime.UtcNow
                    };

                    _context.Set<Leaderboard>().Add(entry);
                    await _context.SaveChangesAsync();
                }

                return entry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting or creating leaderboard entry for user {UserId}", userId);
                throw;
            }
        }
    }
}
