/*
| Module      : Collaboration
| Entity      : Checklist
| Purpose     : مدیریت Checklistهای هر Task.
*/

namespace SmartTask.Web.Models.Entities
{
    public class Checklist : BaseEntity
    {
        // Properties

        public int TaskItemId { get; set; }

        public string Title { get; set; } = null!;

        // Navigation Properties

        public TaskItem TaskItem { get; set; } = null!;

        public ICollection<ChecklistItem> Items { get; set; } = new List<ChecklistItem>();
    }
}