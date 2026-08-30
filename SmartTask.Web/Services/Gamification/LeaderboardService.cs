/*
| Module      : Gamification
| Class       : LeaderboardService
| Purpose     : پیاده‌سازی سرویس رتبه‌بندی و محاسبه رتبه‌ها
*/

using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.ViewModels.Gamification;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace SmartTask.Web.Services.Gamification
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LeaderboardService> _logger;
        private readonly IMemoryCache _cache;
        private const string GLOBAL_LEADERBOARD_CACHE_KEY = "leaderboard_global_{0}_{1}";
        private const string WORKSPACE_LEADERBOARD_CACHE_KEY = "leaderboard_workspace_{0}_{1}_{2}";
        private const string TEAM_LEADERBOARD_CACHE_KEY = "leaderboard_teams_{0}_{1}";
        private const int CACHE_DURATION_MINUTES = 60;

        public LeaderboardService(ApplicationDbContext context, ILogger<LeaderboardService> logger, IMemoryCache cache)
        {
            _context = context;
            _logger = logger;
            _cache = cache;
        }

        public async Task<(List<LeaderboardEntryDto> Entries, int TotalCount)> GetGlobalLeaderboardAsync(
            int page = 1, int pageSize = 50, string timeRange = "all")
        {
            try
            {
                var cacheKey = string.Format(GLOBAL_LEADERBOARD_CACHE_KEY, page, timeRange);

                if (_cache.TryGetValue(cacheKey, out (List<LeaderboardEntryDto>, int) cachedResult))
                {
                    return cachedResult;
                }

                var query = _context.Set<Leaderboard>()
                    .Where(l => l.WorkspaceId == null)
                    .OrderBy(l => l.GlobalRank);

                var totalCount = await query.CountAsync();

                var entries = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(l => l.User)
                    .ToListAsync();

                var dtos = entries.Select(entry => MapToLeaderboardEntryDto(entry, timeRange)).ToList();

                var result = (dtos, totalCount);
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching global leaderboard");
                return (new List<LeaderboardEntryDto>(), 0);
            }
        }

        public async Task<(List<LeaderboardEntryDto> Entries, int TotalCount)> GetWorkspaceLeaderboardAsync(
            int workspaceId, int page = 1, int pageSize = 50, string timeRange = "all")
        {
            try
            {
                var cacheKey = string.Format(WORKSPACE_LEADERBOARD_CACHE_KEY, workspaceId, page, timeRange);

                if (_cache.TryGetValue(cacheKey, out (List<LeaderboardEntryDto>, int) cachedResult))
                {
                    return cachedResult;
                }

                var query = _context.Set<Leaderboard>()
                    .Where(l => l.WorkspaceId == workspaceId)
                    .OrderBy(l => l.WorkspaceRank);

                var totalCount = await query.CountAsync();

                var entries = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(l => l.User)
                    .ToListAsync();

                var dtos = entries.Select(entry => MapToLeaderboardEntryDto(entry, timeRange)).ToList();

                var result = (dtos, totalCount);
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching workspace leaderboard for workspace {WorkspaceId}", workspaceId);
                return (new List<LeaderboardEntryDto>(), 0);
            }
        }

        public async Task<(List<TeamLeaderboardDto> Entries, int TotalCount)> GetTeamLeaderboardAsync(
            int workspaceId, string timeRange = "all")
        {
            try
            {
                var cacheKey = string.Format(TEAM_LEADERBOARD_CACHE_KEY, workspaceId, timeRange);

                if (_cache.TryGetValue(cacheKey, out (List<TeamLeaderboardDto>, int) cachedResult))
                {
                    return cachedResult;
                }

                var teamLeaderboards = await _context.Set<TeamLeaderboard>()
                    .Where(tl => tl.WorkspaceId == workspaceId)
                    .OrderBy(tl => tl.TeamRank)
                    .Include(tl => tl.Team)
                    .ToListAsync();

                var dtos = new List<TeamLeaderboardDto>();
                foreach (var teamLb in teamLeaderboards)
                {
                    var dto = MapToTeamLeaderboardDto(teamLb);

                    // Get top 3 members of the team
                    var topMembers = await _context.Set<Leaderboard>()
                        .Where(l => l.WorkspaceId == workspaceId &&
                                   _context.Set<TeamMember>()
                                       .Any(tm => tm.TeamId == teamLb.TeamId && tm.ApplicationUserId == l.UserId))
                        .OrderByDescending(l => l.TotalPoints)
                        .Take(3)
                        .Include(l => l.User)
                        .ToListAsync();

                    dto.TopMembers = topMembers.Select(m => new TeamMemberSummaryDto
                    {
                        UserId = m.UserId,
                        UserName = m.User?.UserName ?? "Unknown",
                        CurrentLevel = m.CurrentLevel,
                        TotalPoints = m.TotalPoints,
                        Rank = m.GlobalRank
                    }).ToList();

                    dtos.Add(dto);
                }

                var result = (dtos, dtos.Count);
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(CACHE_DURATION_MINUTES));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching team leaderboard for workspace {WorkspaceId}", workspaceId);
                return (new List<TeamLeaderboardDto>(), 0);
            }
        }

        public async Task<LeaderboardUserContextDto> GetUserLeaderboardContextAsync(int userId, string timeRange = "all")
        {
            try
            {
                var userEntry = await _context.Set<Leaderboard>()
                    .FirstOrDefaultAsync(l => l.UserId == userId && l.WorkspaceId == null);

                if (userEntry == null)
                {
                    _logger.LogWarning("Leaderboard entry not found for user {UserId}", userId);
                    return new LeaderboardUserContextDto();
                }

                var userRank = userEntry.GlobalRank;
                var neighborCount = 2;

                var neighbors = await _context.Set<Leaderboard>()
                    .Where(l => l.WorkspaceId == null &&
                               (l.GlobalRank >= userRank - neighborCount && l.GlobalRank <= userRank + neighborCount))
                    .OrderBy(l => l.GlobalRank)
                    .Include(l => l.User)
                    .ToListAsync();

                var totalUsers = await _context.Set<Leaderboard>()
                    .Where(l => l.WorkspaceId == null)
                    .CountAsync();

                return new LeaderboardUserContextDto
                {
                    CurrentUser = MapToLeaderboardEntryDto(userEntry, timeRange, true),
                    Neighbors = neighbors
                        .Where(n => n.UserId != userId)
                        .Select(n => MapToLeaderboardEntryDto(n, timeRange))
                        .ToList(),
                    TotalUsersInLeaderboard = totalUsers
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching leaderboard context for user {UserId}", userId);
                return new LeaderboardUserContextDto();
            }
        }

        public async Task<(int TeamRank, int TotalTeams)> GetUserTeamRankAsync(int userId)
        {
            try
            {
                // Find user's team
                var userTeam = await _context.Set<TeamMember>()
                    .FirstOrDefaultAsync(tm => tm.ApplicationUserId == userId);

                if (userTeam == null)
                {
                    return (0, 0);
                }

                var teamLeaderboard = await _context.Set<TeamLeaderboard>()
                    .FirstOrDefaultAsync(tl => tl.TeamId == userTeam.TeamId);

                if (teamLeaderboard == null)
                {
                    return (0, 0);
                }

                var totalTeams = await _context.Set<TeamLeaderboard>()
                    .Where(tl => tl.WorkspaceId == teamLeaderboard.WorkspaceId)
                    .CountAsync();

                return (teamLeaderboard.TeamRank, totalTeams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching team rank for user {UserId}", userId);
                return (0, 0);
            }
        }

        public async Task<LeaderboardEntryDto?> GetUserLeaderboardEntryAsync(int userId, int? workspaceId = null)
        {
            try
            {
                var entry = await _context.Set<Leaderboard>()
                    .FirstOrDefaultAsync(l => l.UserId == userId &&
                                             (workspaceId == null ? l.WorkspaceId == null : l.WorkspaceId == workspaceId));

                return entry != null ? MapToLeaderboardEntryDto(entry, "all") : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching leaderboard entry for user {UserId}", userId);
                return null;
            }
        }

        public async Task RecalculateAllLeaderboardsAsync()
        {
            try
            {
                _logger.LogInformation("Starting leaderboard recalculation");

                // Recalculate global leaderboard
                await RecalculateGlobalLeaderboardAsync();

                // Recalculate workspace leaderboards
                var workspaces = await _context.Workspaces.ToListAsync();
                foreach (var workspace in workspaces)
                {
                    await RecalculateWorkspaceLeaderboardAsync(workspace.Id);
                }

                // Recalculate team leaderboards
                foreach (var workspace in workspaces)
                {
                    await RecalculateTeamLeaderboardAsync(workspace.Id);
                }

                // Clear all caches
                ClearLeaderboardCache();

                _logger.LogInformation("Leaderboard recalculation completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during leaderboard recalculation");
            }
        }

        private async Task RecalculateGlobalLeaderboardAsync()
        {
            var userProgressions = await _context.Set<UserProgression>()
                .Include(up => up.User)
                .OrderByDescending(up => up.TotalExperience)
                .ThenByDescending(up => up.CurrentLevel)
                .ToListAsync();

            var globalLeaderboards = await _context.Set<Leaderboard>()
                .Where(l => l.WorkspaceId == null)
                .ToListAsync();

            var leaderboardDict = globalLeaderboards.ToDictionary(l => l.UserId);

            int rank = 1;
            foreach (var progression in userProgressions)
            {
                if (!leaderboardDict.TryGetValue(progression.UserId, out var leaderboard))
                {
                    leaderboard = new Leaderboard
                    {
                        UserId = progression.UserId,
                        User = progression.User,
                        WorkspaceId = null
                    };
                    _context.Set<Leaderboard>().Add(leaderboard);
                    leaderboardDict[progression.UserId] = leaderboard;
                }

                int previousRank = leaderboard.GlobalRank;
                leaderboard.GlobalRank = rank;
                leaderboard.TotalPoints = progression.TotalExperience;
                leaderboard.CurrentLevel = progression.CurrentLevel;
                leaderboard.TotalExperience = progression.TotalExperience;
                leaderboard.TasksCompleted = progression.TasksCompleted;
                leaderboard.ProjectsCompleted = progression.ProjectsCompleted;
                leaderboard.RankChangeFromPrevious = previousRank - rank;
                leaderboard.LastUpdated = DateTime.UtcNow;
                leaderboard.CalculatedAt = DateTime.UtcNow;

                // Calculate weekly and monthly points
                await UpdateTimeRangePoints(leaderboard);

                rank++;
            }

            await _context.SaveChangesAsync();
        }

        private async Task RecalculateWorkspaceLeaderboardAsync(int workspaceId)
        {
            var workspaceMembers = await _context.Set<WorkspaceMember>()
                .Where(wm => wm.WorkspaceId == workspaceId)
                .Select(wm => wm.ApplicationUserId)
                .ToListAsync();

            var userProgressions = await _context.Set<UserProgression>()
                .Where(up => workspaceMembers.Contains(up.UserId))
                .Include(up => up.User)
                .OrderByDescending(up => up.TotalExperience)
                .ThenByDescending(up => up.CurrentLevel)
                .ToListAsync();

            var workspaceLeaderboards = await _context.Set<Leaderboard>()
                .Where(l => l.WorkspaceId == workspaceId)
                .ToListAsync();

            var leaderboardDict = workspaceLeaderboards.ToDictionary(l => l.UserId);

            int rank = 1;
            foreach (var progression in userProgressions)
            {
                if (!leaderboardDict.TryGetValue(progression.UserId, out var leaderboard))
                {
                    leaderboard = new Leaderboard
                    {
                        UserId = progression.UserId,
                        User = progression.User,
                        WorkspaceId = workspaceId
                    };
                    _context.Set<Leaderboard>().Add(leaderboard);
                    leaderboardDict[progression.UserId] = leaderboard;
                }

                int previousRank = leaderboard.WorkspaceRank;
                leaderboard.WorkspaceRank = rank;
                leaderboard.TotalPoints = progression.TotalExperience;
                leaderboard.CurrentLevel = progression.CurrentLevel;
                leaderboard.TotalExperience = progression.TotalExperience;
                leaderboard.TasksCompleted = progression.TasksCompleted;
                leaderboard.ProjectsCompleted = progression.ProjectsCompleted;
                leaderboard.RankChangeFromPrevious = previousRank - rank;
                leaderboard.LastUpdated = DateTime.UtcNow;
                leaderboard.CalculatedAt = DateTime.UtcNow;

                await UpdateTimeRangePoints(leaderboard);

                rank++;
            }

            await _context.SaveChangesAsync();
        }

        private async Task RecalculateTeamLeaderboardAsync(int workspaceId)
        {
            var teams = await _context.Set<Team>()
                .Where(t => t.WorkspaceId == workspaceId && !t.IsArchived)
                .ToListAsync();

            var teamLeaderboards = await _context.Set<TeamLeaderboard>()
                .Where(tl => tl.WorkspaceId == workspaceId)
                .ToListAsync();

            var teamLbDict = teamLeaderboards.ToDictionary(tl => tl.TeamId);

            var teamScores = new List<(int TeamId, int TotalPoints, int AverageLevel, int TasksCompleted, int ProjectsCompleted, int AchievementsUnlocked, int ActiveMembers)>();

            foreach (var team in teams)
            {
                var teamMembers = await _context.Set<TeamMember>()
                    .Where(tm => tm.TeamId == team.Id)
                    .Select(tm => tm.ApplicationUserId)
                    .ToListAsync();

                if (teamMembers.Count == 0) continue;

                var memberProgressions = await _context.Set<UserProgression>()
                    .Where(up => teamMembers.Contains(up.UserId))
                    .ToListAsync();

                var totalPoints = memberProgressions.Sum(mp => mp.TotalExperience);
                var averageLevel = memberProgressions.Any() ? (int)memberProgressions.Average(mp => mp.CurrentLevel) : 1;
                var tasksCompleted = memberProgressions.Sum(mp => mp.TasksCompleted);
                var projectsCompleted = memberProgressions.Sum(mp => mp.ProjectsCompleted);

                var achievementsUnlocked = await _context.Set<UserAchievement>()
                    .Where(ua => teamMembers.Contains(ua.UserId) && ua.UnlockedDate != null)
                    .CountAsync();

                // Active members this week
                var activeMembers = await _context.Set<UserProgression>()
                    .Where(up => teamMembers.Contains(up.UserId) &&
                                up.LastProgressUpdate >= DateTime.UtcNow.AddDays(-7))
                    .CountAsync();

                teamScores.Add((team.Id, totalPoints, averageLevel, tasksCompleted, projectsCompleted, achievementsUnlocked, activeMembers));
            }

            // Sort by points and assign ranks
            var rankedTeams = teamScores
                .OrderByDescending(ts => ts.TotalPoints)
                .ThenByDescending(ts => ts.AverageLevel)
                .ToList();

            int rank = 1;
            foreach (var (teamId, totalPoints, averageLevel, tasksCompleted, projectsCompleted, achievementsUnlocked, activeMembers) in rankedTeams)
            {
                if (!teamLbDict.TryGetValue(teamId, out var teamLeaderboard))
                {
                    teamLeaderboard = new TeamLeaderboard
                    {
                        TeamId = teamId,
                        WorkspaceId = workspaceId
                    };
                    _context.Set<TeamLeaderboard>().Add(teamLeaderboard);
                    teamLbDict[teamId] = teamLeaderboard;
                }

                int previousRank = teamLeaderboard.TeamRank;
                teamLeaderboard.TeamRank = rank;
                teamLeaderboard.TotalTeamPoints = totalPoints;
                teamLeaderboard.AverageTeamLevel = averageLevel;
                teamLeaderboard.TasksCompleted = tasksCompleted;
                teamLeaderboard.ProjectsCompleted = projectsCompleted;
                teamLeaderboard.AchievementsUnlocked = achievementsUnlocked;
                teamLeaderboard.TeamMemberCount = (await _context.Set<TeamMember>().Where(tm => tm.TeamId == teamId).CountAsync());
                teamLeaderboard.ActiveMembersThisWeek = activeMembers;
                teamLeaderboard.RankChangeFromPrevious = previousRank - rank;
                teamLeaderboard.LastUpdated = DateTime.UtcNow;
                teamLeaderboard.CalculatedAt = DateTime.UtcNow;

                // Calculate productivity metrics
                if (teamLeaderboard.TeamMemberCount > 0)
                {
                    teamLeaderboard.AverageProductivity = (double)totalPoints / teamLeaderboard.TeamMemberCount / 7; // Per member per day
                }

                rank++;
            }

            await _context.SaveChangesAsync();
        }

        private async Task UpdateTimeRangePoints(Leaderboard leaderboard)
        {
            var now = DateTime.UtcNow;

            // Check if we need to reset weekly points (Sunday)
            if (now.DayOfWeek == DayOfWeek.Sunday &&
                (now - leaderboard.WeeklyPointsResetDate).TotalDays >= 1)
            {
                leaderboard.WeeklyPoints = 0;
                leaderboard.WeeklyPointsResetDate = now;
            }

            // Check if we need to reset monthly points (1st of month)
            if (now.Day == 1 &&
                (now - leaderboard.MonthlyPointsResetDate).TotalDays >= 1)
            {
                leaderboard.MonthlyPoints = 0;
                leaderboard.MonthlyPointsResetDate = now;
            }

            // Get recent wallet transactions for this user to calculate time-range points
            var recentTransactions = await _context.Set<WalletTransaction>()
                .Where(wt => wt.UserWallet.UserId == leaderboard.UserId &&
                            wt.TransactionType == Models.Enums.TransactionType.Earned)
                .ToListAsync();

            leaderboard.WeeklyPoints = recentTransactions
                .Where(t => t.TransactionDate >= now.AddDays(-7))
                .Sum(t => t.Amount);

            leaderboard.MonthlyPoints = recentTransactions
                .Where(t => t.TransactionDate >= now.AddMonths(-1))
                .Sum(t => t.Amount);
        }

        private LeaderboardEntryDto MapToLeaderboardEntryDto(Leaderboard entry, string timeRange, bool isCurrentUser = false)
        {
            var points = timeRange switch
            {
                "week" => entry.WeeklyPoints,
                "month" => entry.MonthlyPoints,
                _ => entry.TotalPoints
            };

            return new LeaderboardEntryDto
            {
                UserId = entry.UserId,
                UserName = entry.User?.UserName ?? "Unknown",
                UserAvatar = entry.User?.Avatar,
                UserEmail = entry.User?.Email,
                GlobalRank = entry.GlobalRank,
                WorkspaceRank = entry.WorkspaceRank,
                CurrentLevel = entry.CurrentLevel,
                TotalPoints = points,
                TotalExperience = entry.TotalExperience,
                TasksCompleted = entry.TasksCompleted,
                ProjectsCompleted = entry.ProjectsCompleted,
                AchievementsUnlocked = entry.AchievementsUnlocked,
                ConsecutiveCompletionDays = entry.ConsecutiveCompletionDays,
                WeeklyPoints = entry.WeeklyPoints,
                MonthlyPoints = entry.MonthlyPoints,
                RankChangeFromPrevious = entry.RankChangeFromPrevious,
                IsCurrentUser = isCurrentUser,
                LastUpdated = entry.LastUpdated
            };
        }

        private TeamLeaderboardDto MapToTeamLeaderboardDto(TeamLeaderboard entry)
        {
            return new TeamLeaderboardDto
            {
                TeamId = entry.TeamId,
                TeamName = entry.Team?.Name ?? "Unknown",
                TeamLogo = entry.Team?.Logo,
                TeamColor = entry.Team?.Color,
                TeamRank = entry.TeamRank,
                TotalTeamPoints = entry.TotalTeamPoints,
                AverageTeamLevel = entry.AverageTeamLevel,
                TotalTeamExperience = entry.TotalTeamExperience,
                TasksCompleted = entry.TasksCompleted,
                ProjectsCompleted = entry.ProjectsCompleted,
                TeamMemberCount = entry.TeamMemberCount,
                AchievementsUnlocked = entry.AchievementsUnlocked,
                WeeklyPoints = entry.WeeklyPoints,
                MonthlyPoints = entry.MonthlyPoints,
                AverageCompletionRate = entry.AverageCompletionRate,
                AverageProductivity = entry.AverageProductivity,
                ActiveMembersThisWeek = entry.ActiveMembersThisWeek,
                RankChangeFromPrevious = entry.RankChangeFromPrevious,
                LastUpdated = entry.LastUpdated
            };
        }

        private void ClearLeaderboardCache()
        {
            // In a real implementation, you'd need to iterate through and clear specific keys
            // For now, this is a placeholder. Consider using a distributed cache with key patterns
        }
    }
}
