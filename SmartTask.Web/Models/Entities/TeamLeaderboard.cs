/*
| Module      : Gamification
| Entity      : TeamLeaderboard
| Purpose     : ردیابی رتبه‌بندی تیم‌ها و نمایش عملکرد جمعی
*/

namespace SmartTask.Web.Models.Entities
{
    public class TeamLeaderboard : BaseEntity
    {
        // Foreign Keys
        public int TeamId { get; set; }
        public int WorkspaceId { get; set; }

        // Ranking Information
        public int TeamRank { get; set; }
        public int TotalTeamPoints { get; set; } = 0;
        public int AverageTeamLevel { get; set; } = 1;
        public int TotalTeamExperience { get; set; } = 0;

        // Performance Metrics
        public int TasksCompleted { get; set; } = 0;
        public int ProjectsCompleted { get; set; } = 0;
        public int TeamMemberCount { get; set; } = 0;
        public int AchievementsUnlocked { get; set; } = 0;

        // Time Range Data
        public int WeeklyPoints { get; set; } = 0;
        public int MonthlyPoints { get; set; } = 0;
        public DateTime WeeklyPointsResetDate { get; set; } = DateTime.UtcNow;
        public DateTime MonthlyPointsResetDate { get; set; } = DateTime.UtcNow;

        // Team Health Metrics
        public double AverageCompletionRate { get; set; } = 0.0; // Percentage
        public double AverageProductivity { get; set; } = 0.0; // Points per member per day
        public int ActiveMembersThisWeek { get; set; } = 0;

        // Ranking Metadata
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
        public int RankChangeFromPrevious { get; set; } = 0; // Positive = up, Negative = down

        // Navigation
        public Team Team { get; set; } = null!;
        public Workspace Workspace { get; set; } = null!;
    }
}
