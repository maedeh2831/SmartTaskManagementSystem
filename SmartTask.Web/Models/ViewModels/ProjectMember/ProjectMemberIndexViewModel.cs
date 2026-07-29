using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartTask.Web.Models.ViewModels.ProjectMember;

public class ProjectMemberIndexViewModel
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = null!;
    public string ProjectKey { get; set; } = null!;
    public bool CanManage { get; set; }
    public List<ProjectMemberViewModel> Members { get; set; } = new();
    public List<SelectListItem> AvailableWorkspaceMembers { get; set; } = new();
}