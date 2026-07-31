using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.DTOs;

namespace SmartTask.Web.Services.Interfaces;

public interface ISprintService : IBaseService<Sprint>
{
    Task<Sprint?> GetDetailsAsync(int id);

    Task<List<Sprint>> GetByProjectAsync(int projectId);

    Task<bool> ExistsByNameAsync(
        int projectId,
        string name,
        int? excludeId = null);

    Task<bool> HasDateOverlapAsync(
        int projectId,
        DateTime startDate,
        DateTime endDate,
        int? excludeId = null);

    Task<bool> CanManageSprintsAsync(int projectId, int userId);

    Task<bool> CanManageSprintAsync(int sprintId, int userId);

    Task ActivateAsync(int sprintId);

    Task CompleteAsync(int sprintId);

    Task<List<BurndownPointDto>> GetBurndownDataAsync(int sprintId);
    Task<List<VelocityPointDto>> GetVelocityDataAsync(int projectId, int lastCount = 6);
}