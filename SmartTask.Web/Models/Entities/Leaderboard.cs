/*
| Module      : Gamification
| Entity      : Leaderboard
| Purpose     : ردیابی رتبه‌بندی کاربران در سطح سراسری و Workspace
*/

namespace SmartTask.Web.Models.Entities
{
    public class Leaderboard : BaseEntity
    {
        // Foreign Keys
        public int UserId { get; set; }
        public int? WorkspaceId { get; set; }

        // Ranking Information
        public int GlobalRank { get; set; }
        public int WorkspaceRank { get; set; }
        public int TotalPoints { get; set; } = 0;
        public int CurrentLevel { get; set; } = 1;
        public int TotalExperience { get; set; } = 0;

        // Performance Metrics
        public int TasksCompleted { get; set; } = 0;
        public int ProjectsCompleted { get; set; } = 0;
        public int AchievementsUnlocked { get; set; } = 0;
        public int ConsecutiveCompletionDays { get; set; } = 0;

        // Time Range Data
        public int WeeklyPoints { get; set; } = 0;
        public int MonthlyPoints { get; set; } = 0;
        public DateTime WeeklyPointsResetDate { get; set; } = DateTime.UtcNow;
        public DateTime MonthlyPointsResetDate { get; set; } = DateTime.UtcNow;

        // Ranking Metadata
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
        public int RankChangeFromPrevious { get; set; } = 0; // Positive = up, Negative = down

        // Navigation
        public ApplicationUser User { get; set; } = null!;
        public Workspace? Workspace { get; set; }
    }
}
