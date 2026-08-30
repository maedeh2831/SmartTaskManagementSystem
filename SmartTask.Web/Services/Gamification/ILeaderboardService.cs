/*
| Module      : Gamification
| Interface   : ILeaderboardService
| Purpose     : تعریف قراردادهای سرویس رتبه‌بندی
*/

using SmartTask.Web.Models.ViewModels.Gamification;

namespace SmartTask.Web.Services.Gamification
{
    public interface ILeaderboardService
    {
        // Global Leaderboard
        Task<(List<LeaderboardEntryDto> Entries, int TotalCount)> GetGlobalLeaderboardAsync(int page = 1, int pageSize = 50, string timeRange = "all");

        // Workspace Leaderboard
        Task<(List<LeaderboardEntryDto> Entries, int TotalCount)> GetWorkspaceLeaderboardAsync(int workspaceId, int page = 1, int pageSize = 50, string timeRange = "all");

        // Team Leaderboard
        Task<(List<TeamLeaderboardDto> Entries, int TotalCount)> GetTeamLeaderboardAsync(int workspaceId, string timeRange = "all");

        // User Rank and Neighbors
        Task<LeaderboardUserContextDto> GetUserLeaderboardContextAsync(int userId, string timeRange = "all");

        // User's Team Rank
        Task<(int TeamRank, int TotalTeams)> GetUserTeamRankAsync(int userId);

        // Recalculate all leaderboards
        Task RecalculateAllLeaderboardsAsync();

        // Get user's leaderboard entry
        Task<LeaderboardEntryDto?> GetUserLeaderboardEntryAsync(int userId, int? workspaceId = null);
    }
}
