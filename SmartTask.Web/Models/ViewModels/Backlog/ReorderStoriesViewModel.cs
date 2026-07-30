namespace SmartTask.Web.Models.ViewModels.Backlog;

public class ReorderStoriesViewModel
{
    public int ProjectId { get; set; }
    public List<int> OrderedIds { get; set; } = new();
}