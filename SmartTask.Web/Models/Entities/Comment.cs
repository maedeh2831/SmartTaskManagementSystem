/*
| Module      : Collaboration
| Entity      : Comment
| Purpose     : ثبت نظرات کاربران روی Taskها.
*/

namespace SmartTask.Web.Models.Entities
{
    public class Comment : BaseEntity
    {
        // Properties

        public int TaskItemId { get; set; }

        public int ApplicationUserId { get; set; }

        public string Content { get; set; } = null!;

        public bool IsEdited { get; set; } = false;

        // Navigation Properties

        public TaskItem TaskItem { get; set; } = null!;

        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}