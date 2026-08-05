namespace SmartTask.Web.Models.ViewModels.Notification
{
    public class NotificationIndexViewModel
    {
        public int UnreadCount { get; set; }
        public List<NotificationItemViewModel> Notifications { get; set; } = new();
    }
}