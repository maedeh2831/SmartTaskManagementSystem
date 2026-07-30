using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Services.Interfaces;

public interface IProjectService : IBaseService<Project>
{
    Task<Project?> GetDetailsAsync(int id);

    Task<bool> ExistsByKeyAsync(int workspaceId, string key, int? excludeId = null);

    Task<bool> CanManageProjectsAsync(int workspaceId, int userId);

    Task<bool> CanManageProjectAsync(int projectId, int userId);

    Task ArchiveAsync(int id);

    Task RestoreAsync(int id);

    Task UpdatePreferencesAsync(int id, string color, string icon);
}