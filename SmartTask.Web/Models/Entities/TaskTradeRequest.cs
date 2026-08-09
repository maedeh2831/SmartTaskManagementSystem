/*
| Module      : Collaboration
| Entity      : TaskTradeRequest
| Purpose     : مدیریت درخواست‌های مبادله/واگذاری تسک بین اعضای پروژه.
*/
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities
{
    public class TaskTradeRequest : BaseEntity
    {
        public int ProjectId { get; set; }
        public int RequesterUserId { get; set; }
        public int TargetUserId { get; set; }
        public int RequesterTaskId { get; set; }
        public int? TargetTaskId { get; set; }
        public string? Message { get; set; }
        public TradeRequestStatusType Status { get; set; } = TradeRequestStatusType.Pending;
        public DateTime? ResponseDate { get; set; }

        public Project Project { get; set; } = null!;
        public ApplicationUser RequesterUser { get; set; } = null!;
        public ApplicationUser TargetUser { get; set; } = null!;
        public TaskItem RequesterTask { get; set; } = null!;
        public TaskItem? TargetTask { get; set; }
    }
}