namespace SmartTask.Web.Models.ViewModels.Dependency;
public class DependencyGraphEdgeViewModel
{
    public int SourceTaskId { get; set; }
    public int TargetTaskId { get; set; }
    public bool IsRequired { get; set; }
}