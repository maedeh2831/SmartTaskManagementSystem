using SmartTask.Web.Models.ViewModels.ProjectDashboard;

namespace SmartTask.Web.Services.Interfaces;

public interface IProjectDashboardService
{
    Task<ProjectDashboardViewModel?> GetDashboardAsync(int projectId);
}