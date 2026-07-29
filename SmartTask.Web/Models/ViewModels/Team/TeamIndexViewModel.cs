namespace SmartTask.Web.Models.ViewModels.Team;

public class TeamIndexViewModel
{
    public int WorkspaceId { get; set; }
    public string WorkspaceName { get; set; } = null!;
    public bool CanManageTeams { get; set; }
    public List<TeamListItemViewModel> Teams { get; set; } = new();
}