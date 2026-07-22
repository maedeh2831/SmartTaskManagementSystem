/*
| Module      : Collaboration
| Entity      : TaskLabel
| Purpose     : ارتباط چند به چند بین Task و Label.
*/

namespace SmartTask.Web.Models.Entities
{
    public class TaskLabel : BaseEntity
    {
        // Properties

        public int TaskItemId { get; set; }

        public int LabelId { get; set; }

        // Navigation Properties

        public TaskItem TaskItem { get; set; } = null!;

        public Label Label { get; set; } = null!;
    }
}