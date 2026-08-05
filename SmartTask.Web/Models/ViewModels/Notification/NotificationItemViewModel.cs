using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Notification
{
    public class NotificationItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreateDate { get; set; }

        public string IconClass => Type switch
        {
            NotificationType.Assignment => "fa-solid fa-user-check",
            NotificationType.Comment => "fa-solid fa-comment",
            NotificationType.Mention => "fa-solid fa-at",
            NotificationType.Invitation => "fa-solid fa-user-plus",
            NotificationType.StatusChange => "fa-solid fa-arrows-rotate",
            NotificationType.Deadline => "fa-solid fa-hourglass-end",
            NotificationType.Reminder => "fa-solid fa-bell",
            _ => "fa-solid fa-circle-info"
        };

        public string TimeAgoDisplay => GetTimeAgo(CreateDate);

        private static string GetTimeAgo(DateTime date)
        {
            var span = DateTime.UtcNow - date;

            if (span.TotalMinutes < 1) return "همین الان";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} دقیقه پیش";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} ساعت پیش";
            if (span.TotalDays < 30) return $"{(int)span.TotalDays} روز پیش";
            return date.ToString("yyyy/MM/dd");
        }
    }
}