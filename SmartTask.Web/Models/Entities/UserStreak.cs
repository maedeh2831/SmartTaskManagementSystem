/*
| Module      : Gamification
| Entity      : UserStreak
| Purpose     : ردیابی رشته‌های بهره‌وری روزانه کاربران
*/

namespace SmartTask.Web.Models.Entities
{
    public class UserStreak : BaseEntity
    {
        // Foreign Keys
        public int UserId { get; set; }

        // Streak Information
        public int CurrentStreak { get; set; } = 0;
        public int LongestStreak { get; set; } = 0;
        public DateTime StreakStartDate { get; set; }
        public DateTime LastCompletionDate { get; set; }

        // Milestone Tracking
        public bool Milestone3Days { get; set; } = false;
        public bool Milestone7Days { get; set; } = false;
        public bool Milestone14Days { get; set; } = false;
        public bool Milestone30Days { get; set; } = false;
        public bool Milestone100Days { get; set; } = false;

        // Daily Stats
        public int TasksCompletedToday { get; set; } = 0;
        public int XpGainedToday { get; set; } = 0;
        public DateTime LastResetDate { get; set; } = DateTime.UtcNow;

        // Timezone Support
        public string UserTimeZone { get; set; } = "UTC";

        // Navigation
        public ApplicationUser User { get; set; } = null!;
    }
}
