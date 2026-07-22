/*
| Module      : Tracking
| Entity      : Notification
| Purpose     : اعلان‌های سیستم برای کاربران.
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities
{
    public class Notification : BaseEntity
    {
        public int ApplicationUserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; }

        public bool IsRead { get; set; }

        public DateTime? ReadDate { get; set; }

        // Navigation Property
        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}