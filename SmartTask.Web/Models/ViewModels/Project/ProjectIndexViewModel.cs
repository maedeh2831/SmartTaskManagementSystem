namespace SmartTask.Web.Models.ViewModels.Project;

public class ProjectIndexViewModel
{
    public int WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = null!;
    public bool CanManageProjects { get; set; }
    public List<ProjectListItemViewModel> Projects { get; set; } = new();
}