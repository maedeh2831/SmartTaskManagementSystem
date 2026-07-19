/*
| Module      : Agile
| Entity      : SubTaskItem
| Purpose     : مدیریت زیرتسک‌های هر Task.
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities
{
    public class SubTaskItem : BaseEntity
    {
        // Properties

        public int TaskItemId { get; set; }

        public string Title { get; set; } = null!;

        public bool IsCompleted { get; set; } = false;

        // Navigation Properties

        public TaskItem TaskItem { get; set; } = null!;
    }
}