using SmartTask.Web.Models.Enums;
namespace SmartTask.Web.Models.ViewModels.Settings
{
    public class NotificationPreferenceItemViewModel
    {
        public NotificationType NotificationType { get; set; }
        public string Title { get; set; } = null!;
        public bool IsEnabled { get; set; }
    }
}