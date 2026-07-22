/*
| Module      : Tracking
| Entity      : TimeLog
| Purpose     : ثبت زمان صرف‌شده روی Taskها.
*/

namespace SmartTask.Web.Models.Entities
{
    public class TimeLog : BaseEntity
    {
        public int TaskItemId { get; set; }

        public int ApplicationUserId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int DurationMinutes { get; set; }

        public string? Description { get; set; }

        // Navigation Properties
        public virtual TaskItem TaskItem { get; set; } = null!;

        public virtual ApplicationUser ApplicationUser { get; set; } = null!;
    }
}