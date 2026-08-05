using SmartTask.Web.Models.ViewModels.Workspace;

namespace SmartTask.Web.Models.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalWorkspaces { get; set; }
        public int TotalProjects { get; set; }
        public int TotalTasks { get; set; }
        public int NewUsersLast7Days { get; set; }
        public int NewWorkspacesLast7Days { get; set; }

        public List<ChartPointViewModel> UserGrowthChart { get; set; } = new();
        public List<ChartPointViewModel> WorkspaceGrowthChart { get; set; } = new();
        public List<AdminTopWorkspaceItemViewModel> TopWorkspaces { get; set; } = new();
    }
}