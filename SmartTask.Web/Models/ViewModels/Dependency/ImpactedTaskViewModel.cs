namespace SmartTask.Web.Models.ViewModels.Dependency;

public class ImpactedTaskViewModel
{
    public int TaskId { get; set; }
    public string Title { get; set; } = null!;
    public int Depth { get; set; }
    public bool IsRequiredChain { get; set; }
    public DateTime? OriginalDueDate { get; set; }
    public DateTime? ProjectedDueDate { get; set; }
}