namespace SmartTask.Web.Models.ViewModels.Task;

public class TaskIndexViewModel
{
    public int UserStoryId { get; set; }
    public string UserStoryTitle { get; set; } = null!;
    public int ProjectId { get; set; }
    public bool CanManage { get; set; }
    public List<TaskListItemViewModel> Tasks { get; set; } = new();
}