using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Services.Interfaces
{
    public interface IReminderService
    {
        Task<List<Reminder>> GetByUserAsync(int userId);
        Task<Reminder?> GetByIdAsync(int id);
        Task<List<TaskItem>> GetAssignedTasksAsync(int userId);
        Task<bool> CanManageReminderAsync(int id, int userId);
        Task CreateAsync(int taskItemId, int userId, string title, DateTime reminderDate);
        Task UpdateAsync(int id, string title, DateTime reminderDate);
        Task DeleteAsync(int id);

        // برای Background Service
        Task<List<Reminder>> GetPendingManualRemindersAsync();
        Task MarkAsSentAsync(int id);
        Task<bool> AutoReminderExistsAsync(int taskItemId, int userId, string marker);
        Task CreateAutoSentReminderAsync(int taskItemId, int userId, string title, DateTime reminderDate);
    }
}