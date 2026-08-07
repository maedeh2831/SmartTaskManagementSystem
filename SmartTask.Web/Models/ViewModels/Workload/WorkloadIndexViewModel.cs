namespace SmartTask.Web.Models.ViewModels.Workload;

public class WorkloadIndexViewModel
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = null!;
    public bool CanManage { get; set; }

    public bool HasActiveSprint { get; set; }
    public string? ActiveSprintName { get; set; }
    public DateTime? ActiveSprintEndDate { get; set; }

    public List<WorkloadMemberViewModel> SprintWorkload { get; set; } = new();
    public List<WorkloadMemberViewModel> ProjectWorkload { get; set; } = new();

    public double SprintUnassignedHours { get; set; }
    public double ProjectUnassignedHours { get; set; }
}