namespace SmartTask.Web.Models.ViewModels.Home
{
    public class UpcomingTaskItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int DaysLeft { get; set; }
    }
}