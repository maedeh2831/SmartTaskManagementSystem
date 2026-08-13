namespace SmartTask.Web.Models.ViewModels.Dependency;
public class DependencyGraphViewModel
{
    public List<DependencyGraphNodeViewModel> Nodes { get; set; } = new();
    public List<DependencyGraphEdgeViewModel> Edges { get; set; } = new();
}