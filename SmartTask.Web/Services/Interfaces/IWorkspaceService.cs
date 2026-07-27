using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Services.Interfaces;

public interface IWorkspaceService : IBaseService<Workspace>
{
    Task<Workspace?> GetDetailsAsync(int id);

    Task<bool> ExistsByNameAsync(
        string name,
        int? excludeId = null);

    Task<bool> IsOwnerAsync(
        int workspaceId,
        int userId);
}