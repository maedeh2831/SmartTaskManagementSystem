using SmartTask.Web.Models.ViewModels.Activity;
using SmartTask.Web.Models.ViewModels.Workspace;

namespace SmartTask.Web.Models.ViewModels.Home
{
    public class UserDashboardViewModel
    {
        public string FullName { get; set; } = string.Empty;

        public int TotalWorkspaces { get; set; }
        public int TotalProjects { get; set; }
        public int TotalAssignedTasks { get; set; }
        public int CompletedAssignedTasks { get; set; }
        public int OverdueAssignedTasks { get; set; }

        public List<ChartPointViewModel> TaskStatusChart { get; set; } = new();
        public List<UpcomingTaskItemViewModel> UpcomingTasks { get; set; } = new();
        public List<DashboardWorkspaceItemViewModel> MyWorkspaces { get; set; } = new();
        public List<ActivityItemViewModel> RecentActivities { get; set; } = new();
    }
}