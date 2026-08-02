using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Services.Interfaces;

public interface ITimeLogService
{
    Task<List<TimeLog>> GetByTaskAsync(int taskItemId);
    Task<TimeLog?> GetActiveTimerAsync(int taskItemId, int userId);
    Task<TimeLog> StartTimerAsync(int taskItemId, int userId);
    Task StopTimerAsync(int timeLogId);
    Task AddManualLogAsync(int taskItemId, int userId, DateTime startTime, int durationMinutes, string? description);
    Task<bool> CanManageLogAsync(int timeLogId, int userId);
    Task DeleteAsync(int id);
    Task<int> GetTotalMinutesForTaskAsync(int taskItemId);
}