using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.ProjectDashboard;

public class RecentMemberViewModel
{
    public string FullName { get; set; } = null!;
    public ProjectRoleType Role { get; set; }
    public DateTime JoinedDate { get; set; }
}