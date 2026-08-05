namespace SmartTask.Web.Models.ViewModels.Reminder
{
    public class ReminderIndexViewModel
    {
        public List<ReminderListItemViewModel> UpcomingReminders { get; set; } = new();
        public List<ReminderListItemViewModel> PastReminders { get; set; } = new();
    }
}