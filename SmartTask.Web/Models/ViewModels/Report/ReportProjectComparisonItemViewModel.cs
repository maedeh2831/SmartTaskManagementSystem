namespace SmartTask.Web.Models.ViewModels.Report
{
    public class ReportProjectComparisonItemViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string Color { get; set; } = "#4F46E5";
        public int TotalTasks { get; set; }
        public int DoneTasks { get; set; }
        public double CompletionPercentage { get; set; }
    }
}