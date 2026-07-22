/*
| Module      : Collaboration
| Entity      : ChecklistItem
| Purpose     : آیتم‌های داخل Checklist.
*/

namespace SmartTask.Web.Models.Entities
{
    public class ChecklistItem : BaseEntity
    {
        // Properties

        public int ChecklistId { get; set; }

        public string Title { get; set; } = null!;

        public bool IsCompleted { get; set; } = false;

        // Navigation Properties

        public Checklist Checklist { get; set; } = null!;
    }
}