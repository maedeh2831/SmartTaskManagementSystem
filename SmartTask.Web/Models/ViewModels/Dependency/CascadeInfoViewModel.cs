namespace SmartTask.Web.Models.ViewModels.Dependency;

public class CascadeInfoViewModel
{
    public int SourceTaskId { get; set; }
    public string SourceTaskTitle { get; set; } = null!;
    public int DelayDaysApplied { get; set; }
    public DateTime AppliedDate { get; set; }
}