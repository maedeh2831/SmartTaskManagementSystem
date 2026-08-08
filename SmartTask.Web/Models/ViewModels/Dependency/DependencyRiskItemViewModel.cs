namespace SmartTask.Web.Models.ViewModels.Dependency;

public class DependencyRiskItemViewModel
{
    public int TaskId { get; set; }
    public string Title { get; set; } = null!;
    public int DelayDays { get; set; }
    public int ImpactedTaskCount { get; set; }
    public List<string> ImpactedTaskTitles { get; set; } = new();
}