/*
| Module      : Workspace
| ViewModel   : WorkspaceDashboardViewModel
| Purpose     : نمایش آمار، پروژه‌های اخیر، فعالیت‌های اخیر و نمودارهای Workspace Dashboard.
*/
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Workspace;

public class WorkspaceDashboardViewModel
{
    public int WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = string.Empty;
    public string WorkspaceColor { get; set; } = "#4F46E5";

    // Statistics Cards
    public int TotalMembers { get; set; }
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int PendingInvitations { get; set; }

    // Widgets
    public List<DashboardProjectItemViewModel> RecentProjects { get; set; } = new();
    public List<DashboardActivityItemViewModel> RecentActivities { get; set; } = new();
    public List<WorkspaceMemberViewModel> TopMembers { get; set; } = new();

    // Charts
    public List<ChartPointViewModel> ProjectStatusChart { get; set; } = new();
    public List<ChartPointViewModel> ActivityChart { get; set; } = new();
}

public class DashboardProjectItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#4F46E5";
    public ProjectStatusType Status { get; set; }
    public DateTime CreateDate { get; set; }
}

public class DashboardActivityItemViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = "fa-solid fa-circle-info";
    public DateTime CreateDate { get; set; }
}

public class ChartPointViewModel
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
}