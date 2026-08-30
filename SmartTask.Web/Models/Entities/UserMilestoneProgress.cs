/*
| Module      : Gamification
| Entity      : UserMilestoneProgress
| Purpose     : پیشرفت کاربران در رسیدن به نقاط عطف
*/

namespace SmartTask.Web.Models.Entities
{
    public class UserMilestoneProgress : BaseEntity
    {
        public int UserId { get; set; }
        public int MilestoneId { get; set; }
        public int CurrentProgress { get; set; }
        public int TargetValue { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime? CompletedDate { get; set; }
        public DateTime LastProgressUpdate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ApplicationUser User { get; set; }
        public Milestone Milestone { get; set; }
    }
}
