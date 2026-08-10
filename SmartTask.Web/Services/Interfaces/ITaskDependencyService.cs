using SmartTask.Web.Models.ViewModels.Dependency;

namespace SmartTask.Web.Services.Interfaces;

public interface ITaskDependencyService
{
    Task<TaskDependencyWidgetViewModel> GetWidgetAsync(int taskId, int currentUserId);
    Task<(bool success, string? error)> AddDependencyAsync(int taskId, int dependsOnTaskId, bool isRequired);
    Task<bool> RemoveDependencyAsync(int id);
    Task<List<DependencyRiskItemViewModel>> GetProjectRiskOverviewAsync(int projectId);
    Task<List<ImpactedTaskViewModel>> GetImpactedTasksAsync(int taskId, int delayDays);
    Task<List<CascadeInfoViewModel>> GetCascadeInfoAsync(int taskId);
    Task<DependencyGraphViewModel> GetDependencyGraphAsync(int projectId);
}