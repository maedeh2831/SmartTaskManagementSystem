/*
| Module      : Common
| Entity      : BaseEntity
| Purpose     : کلاس پایه شامل فیلدهای مشترک تمام موجودیت‌های سامانه.
*/

namespace SmartTask.Web.Models.Entities
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public string? ChangeUser { get; set; }

        public DateTime? ChangeDate { get; set; }

        public bool ViewState { get; set; } = true;
    }
}