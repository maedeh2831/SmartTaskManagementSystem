using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.ProjectDashboard;

public class ProjectDashboardViewModel
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = null!;
    public string ProjectKey { get; set; } = null!;
    public string Color { get; set; } = "#4F46E5";
    public string Icon { get; set; } = "fa-solid fa-diagram-project";
    public ProjectStatusType Status { get; set; }
    public ProjectPriorityType Priority { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public int? DaysRemaining { get; set; }
    public bool IsOverdue { get; set; }
    public int ProgressPercentage { get; set; }
    public int MembersCount { get; set; }
    public int TeamsCount { get; set; }
    public List<RoleDistributionItem> RoleDistribution { get; set; } = new();
    public List<RecentMemberViewModel> RecentMembers { get; set; } = new();
    public List<string> TeamNames { get; set; } = new();
    public SmartTask.Web.Models.ViewModels.Health.ProjectHealthViewModel? Health { get; set; }
}