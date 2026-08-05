namespace SmartTask.Web.Models.ViewModels.Report
{
    public class ReportMemberWorkloadItemViewModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public int AssignedTasksCount { get; set; }
        public int CompletedTasksCount { get; set; }
        public int TotalMinutesLogged { get; set; }
        public double? AvgCompletionHours { get; set; }
    }
}