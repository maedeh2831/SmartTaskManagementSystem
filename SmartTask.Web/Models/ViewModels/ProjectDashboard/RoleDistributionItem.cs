using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.ProjectDashboard;

public class RoleDistributionItem
{
    public ProjectRoleType Role { get; set; }
    public int Count { get; set; }
}