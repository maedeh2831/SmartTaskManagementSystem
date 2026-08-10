namespace SmartTask.Web.Models.ViewModels.Dependency;
public class DependencyGraphNodeViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public bool IsDone { get; set; }
    public bool IsOverdue { get; set; }
    public bool IsAtRisk { get; set; }
}