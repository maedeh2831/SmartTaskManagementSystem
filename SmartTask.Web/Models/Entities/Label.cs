/*
| Module      : Collaboration
| Entity      : Label
| Purpose     : برچسب‌های قابل استفاده روی Taskها.
*/

namespace SmartTask.Web.Models.Entities
{
    public class Label : BaseEntity
    {
        // Properties

        public int ProjectId { get; set; }

        public string Name { get; set; } = null!;

        public string Color { get; set; } = "#2196F3";

        // Navigation Properties

        public Project Project { get; set; } = null!;

        public ICollection<TaskLabel> TaskLabels { get; set; } = new List<TaskLabel>();
    }
}