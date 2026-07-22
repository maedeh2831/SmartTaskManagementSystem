/*
| Module      : Tracking
| Entity      : ActivityLog
| Purpose     : ثبت فعالیت‌های کاربران روی سیستم.
*/

namespace SmartTask.Web.Models.Entities
{
    public class ActivityLog : BaseEntity
    {
        public int ApplicationUserId { get; set; }

        public int? TaskItemId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime ActivityDate { get; set; }

        // Navigation Properties
        public ApplicationUser ApplicationUser { get; set; } = null!;

        public TaskItem? TaskItem { get; set; }
    }
}