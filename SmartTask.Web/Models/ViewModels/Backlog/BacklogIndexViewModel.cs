namespace SmartTask.Web.Models.ViewModels.Backlog;

public class BacklogIndexViewModel
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = null!;
    public bool CanManage { get; set; }
    public List<UserStoryListItemViewModel> Stories { get; set; } = new();
    public List<ProjectMemberOptionViewModel> ProjectMembers { get; set; } = new();
}