using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Services.Interfaces;

public interface ILabelService
{
    Task<List<Label>> GetByProjectAsync(int projectId);
    Task<bool> ExistsByNameAsync(int projectId, string name, int? excludeId = null);
    Task<bool> CanManageLabelsAsync(int projectId, int userId);
    Task CreateOrReactivateAsync(int projectId, string name, string color); // 👈 جایگزین CreateAsync
    Task DeleteAsync(int id);
}