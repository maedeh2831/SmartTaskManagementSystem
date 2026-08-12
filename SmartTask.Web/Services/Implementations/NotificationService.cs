using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Hubs;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(int userId, int take = 50)
        {
            return await _context.Notifications
                .Where(x => x.ApplicationUserId == userId && x.ViewState)
                .OrderByDescending(x => x.CreatedDate)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetRecentAsync(int userId, int count = 8)
        {
            return await _context.Notifications
                .Where(x => x.ApplicationUserId == userId && x.ViewState)
                .OrderByDescending(x => x.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(x => x.ApplicationUserId == userId && x.ViewState && !x.IsRead);
        }

        public async Task CreateAsync(int userId, string title, string message, NotificationType type)
        {
            if (!await IsNotificationEnabledAsync(userId, type))
                return;

            var notification = new Notification
            {
                ApplicationUserId = userId,
                Title = title,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedDate = DateTime.UtcNow,
                ViewState = true
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();

            var unreadCount = await GetUnreadCountAsync(userId);

            await _hubContext.Clients
                .Group(NotificationHub.GetUserGroupName(userId))
                .SendAsync("ReceiveNotification", new
                {
                    id = notification.Id,
                    title = notification.Title,
                    message = notification.Message,
                    type = notification.Type.ToString(),
                    createdDate = notification.CreatedDate,
                    unreadCount
                });
        }

        private async Task<bool> IsNotificationEnabledAsync(int userId, NotificationType type)
        {
            if (type == NotificationType.System || type == NotificationType.Reminder)
                return true;

            var preference = await _context.UserNotificationPreferences
                .FirstOrDefaultAsync(x => x.ApplicationUserId == userId && x.NotificationType == type);

            return preference?.IsEnabled ?? true;
        }

        public async Task<bool> CanManageNotificationAsync(int id, int userId)
        {
            var notification = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == id);
            return notification != null && notification.ApplicationUserId == userId;
        }

        public async Task MarkAsReadAsync(int id, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(x => x.Id == id && x.ApplicationUserId == userId);

            if (notification == null || notification.IsRead)
                return;

            notification.IsRead = true;
            notification.ReadDate = DateTime.UtcNow;
            notification.ChangeDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(x => x.ApplicationUserId == userId && x.ViewState && !x.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
                notification.ReadDate = DateTime.UtcNow;
                notification.ChangeDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id, int userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(x => x.Id == id && x.ApplicationUserId == userId);

            if (notification == null)
                return;

            notification.ViewState = false;
            notification.ChangeDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}