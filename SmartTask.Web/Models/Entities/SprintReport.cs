/*
| Module      : Agile
| Entity      : SprintReport
| Purpose     : ذخیره گزارش‌های هوشمند تولیدشده در پایان هر اسپرینت.
*/
namespace SmartTask.Web.Models.Entities
{
    public class SprintReport : BaseEntity
    {
        public int SprintId { get; set; }
        public string Content { get; set; } = null!;
        public int GeneratedByUserId { get; set; }
        public DateTime GeneratedDate { get; set; } = DateTime.Now;

        public Sprint Sprint { get; set; } = null!;
        public ApplicationUser GeneratedByUser { get; set; } = null!;
    }
}