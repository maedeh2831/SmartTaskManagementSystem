namespace SmartTask.Web.Models.ViewModels.Report
{
    public class ReportOverdueTaskItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
    }
}