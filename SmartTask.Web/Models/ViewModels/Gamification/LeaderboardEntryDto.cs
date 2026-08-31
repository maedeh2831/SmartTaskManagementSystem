/*
| Module      : Gamification
| DTO         : LeaderboardEntryDto
| Purpose     : نمایش اطلاعات رتبه‌بندی کاربر
*/

namespace SmartTask.Web.Models.ViewModels.Gamification
{
    public class LeaderboardEntryDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string? UserAvatar { get; set; }
        public string? UserEmail { get; set; }

        public int GlobalRank { get; set; }
        public int WorkspaceRank { get; set; }
        public int CurrentLevel { get; set; }
        public int TotalPoints { get; set; }
        public int TotalExperience { get; set; }

        public int TasksCompleted { get; set; }
        public int ProjectsCompleted { get; set; }
        public int AchievementsUnlocked { get; set; }
        public int ConsecutiveCompletionDays { get; set; }

        public int WeeklyPoints { get; set; }
        public int MonthlyPoints { get; set; }

        public int RankChangeFromPrevious { get; set; }
        public bool IsCurrentUser { get; set; }
        public DateTime LastUpdated { get; set; }

        // Badge/Achievement highlights
        public string? TopBadge { get; set; }
        public int BadgeCount { get; set; }
    }
}
