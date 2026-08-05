namespace SmartTask.Web.Models.ViewModels.Reminder
{
    public class ReminderListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime ReminderDate { get; set; }
        public bool IsSent { get; set; }
        public int TaskItemId { get; set; }
        public string TaskTitle { get; set; } = string.Empty;
        public bool IsPast => ReminderDate < DateTime.Now;
    }
}