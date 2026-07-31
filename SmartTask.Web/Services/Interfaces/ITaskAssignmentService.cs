using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Services.Interfaces;

public interface ITaskAssignmentService
{
    Task<List<ApplicationUser>> GetAssigneesAsync(int taskItemId);
    Task<bool> IsAssignedAsync(int taskItemId, int userId);
    Task AssignUserAsync(int taskItemId, int userId);
    Task RemoveUserAsync(int taskItemId, int userId);
}