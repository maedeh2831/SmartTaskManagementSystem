using SmartTask.Web.Models.ViewModels.Health;

namespace SmartTask.Web.Services.Interfaces;

public interface IProjectHealthService
{
    Task<ProjectHealthViewModel?> GetHealthAsync(int projectId, int currentUserId);
}