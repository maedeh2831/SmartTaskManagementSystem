/*
| Module      : Gamification
| Entity      : UserSeasonalEventProgress
| Purpose     : ردیابی پیشرفت کاربر در رویدادهای فصلی
*/

namespace SmartTask.Web.Models.Entities
{
    public class UserSeasonalEventProgress : BaseEntity
    {
        // Foreign Keys
        public int UserId { get; set; }
        public int SeasonalEventId { get; set; }

        // Progress
        public int EventPoints { get; set; } = 0;
        public int TasksCompleted { get; set; } = 0;
        public int AchievementsUnlocked { get; set; } = 0;
        public int CurrentRank { get; set; } = 0;

        // Participation
        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public bool HasClaimed { get; set; } = false;
        public DateTime? ClaimedDate { get; set; }

        // Navigation
        public ApplicationUser User { get; set; } = null!;
        public SeasonalEvent SeasonalEvent { get; set; } = null!;
    }
}
