using SmartTask.Web.Models.ViewModels.Workspace;

namespace SmartTask.Web.Services.Interfaces;

public interface IWorkspaceDashboardService
{
    Task<WorkspaceDashboardViewModel> GetDashboardAsync(int workspaceId, int currentUserId);
}