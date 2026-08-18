using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Services.Interfaces
{
    public interface IActivityLogService
    {
        Task LogAsync(int userId, string action, string? description = null, int? taskItemId = null);
        Task BatchLogAsync(List<(int userId, string action, string? description, int? taskItemId)> logs);
        Task<List<ActivityLog>> GetUserActivitiesAsync(int userId, int take = 50);
        Task<List<ActivityLog>> GetTaskActivitiesAsync(int taskItemId);
    }
}