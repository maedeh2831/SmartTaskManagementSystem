/*
| Module      : Identity
| Entity      : UserNotificationPreference
| Purpose     : نگهداری تنظیمات دریافت اعلان به تفکیک نوع، برای هر کاربر.
*/
using SmartTask.Web.Models.Enums;
namespace SmartTask.Web.Models.Entities
{
    public class UserNotificationPreference
    {
        public int Id { get; set; }
        public int ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; } = null!;
        public NotificationType NotificationType { get; set; }
        public bool IsEnabled { get; set; } = true;
    }
}