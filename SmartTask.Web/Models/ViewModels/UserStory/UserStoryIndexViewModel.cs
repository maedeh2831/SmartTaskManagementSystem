using SmartTask.Web.Models.ViewModels.Backlog;

namespace SmartTask.Web.Models.ViewModels.UserStory;

public class UserStoryIndexViewModel
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = null!;
    public bool CanManage { get; set; }
    public List<UserStoryListItemViewModel> Stories { get; set; } = new();
}