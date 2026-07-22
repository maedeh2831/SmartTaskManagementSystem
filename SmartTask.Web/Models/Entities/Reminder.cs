/*
| Module      : Tracking
| Entity      : Reminder
| Purpose     : یادآوری برای Taskها.
*/

namespace SmartTask.Web.Models.Entities
{
    public class Reminder : BaseEntity
    {
        public int TaskItemId { get; set; }

        public int ApplicationUserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public DateTime ReminderDate { get; set; }

        public bool IsSent { get; set; }

        // Navigation Properties
        public virtual TaskItem TaskItem { get; set; } = null!;

        public virtual ApplicationUser ApplicationUser { get; set; } = null!;
    }
}