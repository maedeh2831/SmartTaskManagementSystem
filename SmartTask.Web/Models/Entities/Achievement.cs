/*
| Module      : Gamification
| Entity      : Achievement
| Purpose     : تعریف دستاورد‌های سامانه
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities
{
    public class Achievement : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }

        public AchievementRarity Rarity { get; set; } = AchievementRarity.Common;
        public AchievementCategory Category { get; set; }

        public int RewardPoints { get; set; }
        public int RewardExperience { get; set; }

        public string Condition { get; set; }
        public int ConditionValue { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
    }
}
