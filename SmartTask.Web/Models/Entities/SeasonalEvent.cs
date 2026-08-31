/*
| Module      : Gamification
| Entity      : SeasonalEvent
| Purpose     : مدیریت رویدادهای فصلی محدود‌الزمان
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities
{
    public class SeasonalEvent : BaseEntity
    {
        // Event Information
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }

        // Time Window
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Status
        public EventStatus Status { get; set; } = EventStatus.Scheduled;
        public bool IsActive { get; set; } = false;

        // Bonus Configuration
        public decimal AchievementBonusMultiplier { get; set; } = 1.0m;
        public decimal RewardBonusMultiplier { get; set; } = 1.0m;
        public int ExtraPointsPerCompletion { get; set; } = 0;

        // Event Rules
        public string EligibilityCriteria { get; set; } // JSON serialized rules
        public int MaxParticipants { get; set; } = -1; // -1 = unlimited
        public int CurrentParticipants { get; set; } = 0;

        // Leaderboard
        public bool HasEventLeaderboard { get; set; } = true;

        // Navigation
        public ICollection<UserSeasonalEventProgress> UserProgresses { get; set; } = new List<UserSeasonalEventProgress>();
    }
}
