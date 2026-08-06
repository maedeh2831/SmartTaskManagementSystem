/*
| Module      : Offroad
| Entity      : OffroadTask
| Purpose     : ثبت و پیگیری کارهای ضروری خارج از دامنه رسمی پروژه.
*/
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities
{
    public class OffroadTask : BaseEntity
    {
        // Properties
        public int ProjectId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public OffroadStatusType Status { get; set; } = OffroadStatusType.ToDo;
        public OffroadPriorityType Priority { get; set; } = OffroadPriorityType.Normal;
        public int CreatedByUserId { get; set; }
        public int? AssignedToUserId { get; set; }
        public DateTime? DueDate { get; set; }

        // Navigation Properties
        public Project Project { get; set; } = null!;
        public ApplicationUser CreatedByUser { get; set; } = null!;
        public ApplicationUser? AssignedToUser { get; set; }
    }
}