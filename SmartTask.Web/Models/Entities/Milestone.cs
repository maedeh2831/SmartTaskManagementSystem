/*
| Module      : Gamification
| Entity      : Milestone
| Purpose     : نقاط عطف و مرحله‌های پیشرفت کاربران
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities
{
    public class Milestone : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
        public MilestoneType Type { get; set; }
        public int TargetValue { get; set; }
        public int RewardPoints { get; set; }
        public int RewardExperience { get; set; }
        public string Condition { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }
}
