/*
| Module      : Tracking
| Entity      : OverdueCascadeLog
| Purpose     : ردیابی میزان تأخیر اعمال‌شده از یک Task منبع به Taskهای وابسته، برای جلوگیری از اعمال تکراری.
*/
namespace SmartTask.Web.Models.Entities
{
    public class OverdueCascadeLog : BaseEntity
    {
        public int SourceTaskId { get; set; }
        public int ImpactedTaskId { get; set; }
        public int DelayDaysApplied { get; set; }
        public DateTime AppliedDate { get; set; } = DateTime.Now;
        public TaskItem SourceTask { get; set; } = null!;
        public TaskItem ImpactedTask { get; set; } = null!;
    }
}