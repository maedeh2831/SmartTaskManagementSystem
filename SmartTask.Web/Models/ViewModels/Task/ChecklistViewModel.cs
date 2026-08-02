namespace SmartTask.Web.Models.ViewModels.Task;

public class ChecklistViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public List<ChecklistItemViewModel> Items { get; set; } = new();

    public int TotalItems => Items.Count;
    public int CompletedItems => Items.Count(x => x.IsCompleted);
    public int ProgressPercent => TotalItems == 0 ? 0 : (int)Math.Round((double)CompletedItems / TotalItems * 100);
}