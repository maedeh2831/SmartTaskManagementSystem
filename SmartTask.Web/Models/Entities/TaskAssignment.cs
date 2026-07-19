/*
| Module      : Agile
| Entity      : TaskAssignment
| Purpose     : مدیریت تخصیص کاربران به Taskها.
*/

namespace SmartTask.Web.Models.Entities
{
    public class TaskAssignment : BaseEntity
    {
        // Properties

        public int TaskItemId { get; set; }

        public int ApplicationUserId { get; set; }

        public DateTime AssignedDate { get; set; } = DateTime.Now;

        // Navigation Properties

        public TaskItem TaskItem { get; set; } = null!;

        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}