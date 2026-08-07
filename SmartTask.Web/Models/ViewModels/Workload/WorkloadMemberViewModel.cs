using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Workload;

public class WorkloadMemberViewModel
{
    public int ProjectMemberId { get; set; }
    public int UserId { get; set; }
    public string FullName { get; set; } = null!;
    public ProjectRoleType Role { get; set; }
    public int CapacityHours { get; set; }
    public double AssignedHours { get; set; }
    public int TaskCount { get; set; }
    public int UtilizationPercent { get; set; }
    public string StatusLevel { get; set; } = "balanced"; // under, balanced, overloaded
}