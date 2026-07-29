using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Services.Interfaces;

public interface ITeamService : IBaseService<Team>
{
    Task<Team?> GetDetailsAsync(int id);

    Task<bool> ExistsByNameAsync(
        int workspaceId,
        string name,
        int? excludeId = null);

    Task<bool> CanManageTeamsAsync(int workspaceId, int userId);

    Task<bool> CanManageTeamAsync(int teamId, int userId);
}