namespace SmartTask.Web.Models.ViewModels.TaskTrade;

public class TaskTradeIndexViewModel
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = null!;
    public List<TaskTradeItemViewModel> Incoming { get; set; } = new();
    public List<TaskTradeItemViewModel> Outgoing { get; set; } = new();
}