using SmartTask.Web.Models.ViewModels.Workspace;

namespace SmartTask.Web.Models.ViewModels.Report
{
    public class WorkspaceReportViewModel
    {
        public int WorkspaceId { get; set; }
        public string WorkspaceName { get; set; } = string.Empty;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // ===== Tab 1: Task & Productivity =====
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasksCount { get; set; }
        public double CompletionRate { get; set; }
        public List<ChartPointViewModel> TaskStatusChart { get; set; } = new();
        public List<ChartPointViewModel> TaskPriorityChart { get; set; } = new();
        public List<ChartPointViewModel> TimeLogByProjectChart { get; set; } = new();
        public List<ReportOverdueTaskItemViewModel> TopOverdueTasks { get; set; } = new();

        // ===== Tab 2: Team & Membership =====
        public List<ReportMemberWorkloadItemViewModel> MemberWorkload { get; set; } = new();
        public List<ReportProjectComparisonItemViewModel> ProjectComparison { get; set; } = new();
    }
}