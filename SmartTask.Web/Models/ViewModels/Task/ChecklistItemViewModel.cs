namespace SmartTask.Web.Models.ViewModels.Task;

public class ChecklistItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public bool IsCompleted { get; set; }
}