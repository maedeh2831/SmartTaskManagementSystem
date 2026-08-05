using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Services.Interfaces
{
    public interface INotificationService
    {
        Task<List<Notification>> GetUserNotificationsAsync(int userId, int take = 50);
        Task<List<Notification>> GetRecentAsync(int userId, int count = 8);
        Task<int> GetUnreadCountAsync(int userId);
        Task CreateAsync(int userId, string title, string message, NotificationType type);
        Task<bool> CanManageNotificationAsync(int id, int userId);
        Task MarkAsReadAsync(int id, int userId);
        Task MarkAllAsReadAsync(int userId);
        Task DeleteAsync(int id, int userId);
    }
}