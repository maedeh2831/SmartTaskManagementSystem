/*
| Module      : Gamification
| Entity      : UserAchievement
| Purpose     : رابط کاربر و دستاورد
*/

namespace SmartTask.Web.Models.Entities
{
    public class UserAchievement : BaseEntity
    {
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int UserProgressionId { get; set; }
        public UserProgression UserProgression { get; set; }

        public int AchievementId { get; set; }
        public Achievement Achievement { get; set; }

        public DateTime UnlockedDate { get; set; } = DateTime.UtcNow;
        public int ProgressPercentage { get; set; } = 0;
        public bool IsNotified { get; set; } = false;
    }
}
