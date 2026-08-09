using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartTask.Web.Models.ViewModels.Dependency;

public class TaskDependencyWidgetViewModel
{
    public int TaskId { get; set; }
    public bool CanManage { get; set; }
    public List<DependencyItemViewModel> DependsOn { get; set; } = new();
    public List<DependencyItemViewModel> Dependents { get; set; } = new();
    public List<SelectListItem> AvailableTasks { get; set; } = new();
    public int DelayDays { get; set; }
    public List<ImpactedTaskViewModel> ImpactedTasks { get; set; } = new();
}