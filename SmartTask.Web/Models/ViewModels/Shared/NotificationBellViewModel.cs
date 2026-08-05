using SmartTask.Web.Models.ViewModels.Notification;

namespace SmartTask.Web.Models.ViewModels.Shared
{
    public class NotificationBellViewModel
    {
        public int UnreadCount { get; set; }
        public List<NotificationItemViewModel> RecentNotifications { get; set; } = new();
    }
}