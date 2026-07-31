using Microsoft.AspNetCore.Mvc.Rendering;
namespace SmartTask.Web.Models.ViewModels.Team;
public class TeamDetailsViewModel
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Color { get; set; } = "#4F46E5";
    public string? Logo { get; set; }
    public bool IsPrivate { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreateDate { get; set; }
    public bool CanManage { get; set; }
    public List<TeamMemberViewModel> Members { get; set; } = new();
    public List<ProjectTeamItemViewModel> Projects { get; set; } = new(); // 👈 جایگزین ProjectNames
    public List<SelectListItem> AvailableWorkspaceMembers { get; set; } = new();
    public List<SelectListItem> AvailableProjects { get; set; } = new(); // 👈 جدید
}