namespace SmartTask.Web.Models.ViewModels.Dependency;

public class DependencyRiskIndexViewModel
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = null!;
    public List<DependencyRiskItemViewModel> RiskyTasks { get; set; } = new();
}