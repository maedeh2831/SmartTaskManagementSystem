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
        private const int DefaultTakeCount = 50;
        private const int DefaultRecentCount = 8;

        public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(int userId, int take = DefaultTakeCount)
        {
            if (userId <= 0)
                return new List<Notification>();

            return await _context.Notifications
                .Where(x => x.ApplicationUserId == userId && x.ViewState)
                .OrderByDescending(x => x.CreatedDate)
                .Take(Math.Max(1, take))
                .ToListAsync();
        }

        public async Task<List<Notification>> GetRecentAsync(int userId, int count = DefaultRecentCount)
        {
            if (userId <= 0)
                return new List<Notification>();

            return await _context.Notifications
                .Where(x => x.ApplicationUserId == userId && x.ViewState)
                .OrderByDescending(x => x.CreatedDate)
                .Take(Math.Max(1, count))
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            if (userId <= 0)
                return 0;

            return await _context.Notifications
                .CountAsync(x => x.ApplicationUserId == userId && x.ViewState && !x.IsRead);
        }

        /// <summary>
        /// OPTIMIZED: Get unread counts for multiple users in a single query
        /// </summary>
        public async Task<Dictionary<int, int>> GetUnreadCountsAsync(List<int> userIds)
        {
            if (userIds == null || userIds.Count == 0)
                return new Dictionary<int, int>();

            var validIds = userIds.Where(id => id > 0).Distinct().ToList();
            if (validIds.Count == 0)
                return new Dictionary<int, int>();

            var result = await _context.Notifications
                .Where(x => validIds.Contains(x.ApplicationUserId) && x.ViewState && !x.IsRead)
                .GroupBy(x => x.ApplicationUserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.UserId, x => x.Count);

            // Fill in missing users with 0
            foreach (var userId in validIds.Where(id => !result.ContainsKey(id)))
            {
                result[userId] = 0;
            }

            return result;
        }

        public async Task CreateAsync(int userId, string title, string message, NotificationType type)
        {
            if (userId <= 0 || string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
                return;

            if (!await IsNotificationEnabledAsync(userId, type))
                return;

            var now = DateTime.UtcNow;
            var notification = new Notification
            {
                ApplicationUserId = userId,
                Title = title,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedDate = now,
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

        /// <summary>
        /// OPTIMIZED: Batch create notifications in parallel
        /// </summary>
        public async Task BatchCreateAsync(List<(int userId, string title, string message, NotificationType type)> notifications)
        {
            if (notifications == null || notifications.Count == 0)
                return;

            var validNotifications = notifications
                .Where(x => x.userId > 0 && !string.IsNullOrWhiteSpace(x.title) && !string.IsNullOrWhiteSpace(x.message))
                .ToList();

            if (validNotifications.Count == 0)
                return;

            // OPTIMIZED: Pre-fetch enabled preferences for all users to avoid N queries
            var userIds = validNotifications.Select(x => x.userId).Distinct().ToList();
            var preferences = await _context.UserNotificationPreferences
                .Where(x => userIds.Contains(x.ApplicationUserId))
                .ToListAsync();

            var preferenceDict = preferences
                .GroupBy(x => new { x.ApplicationUserId, x.NotificationType })
                .ToDictionary(g => (g.Key.ApplicationUserId, g.Key.NotificationType), g => g.First().IsEnabled);

            var now = DateTime.UtcNow;
            var toAdd = new List<Notification>();
            var hubTasks = new List<Task>();

            foreach (var notif in validNotifications)
            {
                // Check if notification type is enabled (system/reminder always enabled)
                if (notif.type != NotificationType.System && notif.type != NotificationType.Reminder)
                {
                    var key = (notif.userId, notif.type);
                    if (preferenceDict.TryGetValue(key, out var isEnabled) && !isEnabled)
                        continue;
                }

                var notification = new Notification
                {
                    ApplicationUserId = notif.userId,
                    Title = notif.title,
                    Message = notif.message,
                    Type = notif.type,
                    IsRead = false,
                    CreatedDate = now,
                    ViewState = true
                };

                toAdd.Add(notification);
            }

            if (toAdd.Count == 0)
                return;

            // OPTIMIZED: Single SaveChangesAsync for all notifications
            await _context.Notifications.AddRangeAsync(toAdd);
            await _context.SaveChangesAsync();

            // OPTIMIZED: Send SignalR notifications in parallel
            foreach (var notification in toAdd)
            {
                hubTasks.Add(_hubContext.Clients
                    .Group(NotificationHub.GetUserGroupName(notification.ApplicationUserId))
                    .SendAsync("ReceiveNotification", new
                    {
                        id = notification.Id,
                        title = notification.Title,
                        message = notification.Message,
                        type = notification.Type.ToString(),
                        createdDate = notification.CreatedDate
                    }));
            }

            if (hubTasks.Any())
                await Task.WhenAll(hubTasks);
        }

        /// <summary>
        /// OPTIMIZED: Batch check enabled status for multiple users and notification types
        /// </summary>
        private async Task<Dictionary<(int, NotificationType), bool>> IsNotificationEnabledBatchAsync(
            List<(int userId, NotificationType type)> checks)
        {
            if (checks == null || checks.Count == 0)
                return new Dictionary<(int, NotificationType), bool>();

            var result = new Dictionary<(int, NotificationType), bool>();
            var systemTypes = new[] { NotificationType.System, NotificationType.Reminder };

            var userIds = checks.Select(x => x.userId).Distinct().ToList();
            var notificationTypes = checks.Select(x => x.type).Distinct().ToList();

            // System and reminder notifications are always enabled
            foreach (var check in checks)
            {
                if (systemTypes.Contains(check.type))
                {
                    result[check] = true;
                    continue;
                }
            }

            // Get preferences for non-system types
            var nonSystemChecks = checks
                .Where(x => !systemTypes.Contains(x.type) && !result.ContainsKey(x))
                .ToList();

            if (nonSystemChecks.Count == 0)
                return result;

            var preferences = await _context.UserNotificationPreferences
                .Where(x => userIds.Contains(x.ApplicationUserId) && notificationTypes.Contains(x.NotificationType))
                .ToListAsync();

            var preferenceDict = preferences
                .ToDictionary(x => (x.ApplicationUserId, x.NotificationType), x => x.IsEnabled);

            foreach (var check in nonSystemChecks)
            {
                result[check] = preferenceDict.TryGetValue(check, out var isEnabled) ? isEnabled : true;
            }

            return result;
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
            if (id <= 0 || userId <= 0)
                return false;

            return await _context.Notifications
                .AnyAsync(x => x.Id == id && x.ApplicationUserId == userId);
        }

        /// <summary>
        /// OPTIMIZED: Use ExecuteUpdateAsync instead of load-modify-save pattern
        /// </summary>
        public async Task MarkAsReadAsync(int id, int userId)
        {
            if (id <= 0 || userId <= 0)
                return;

            var now = DateTime.UtcNow;

            await _context.Notifications
                .Where(x => x.Id == id && x.ApplicationUserId == userId && !x.IsRead)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.IsRead, true)
                    .SetProperty(x => x.ReadDate, now)
                    .SetProperty(x => x.ChangeDate, now));
        }

        /// <summary>
        /// OPTIMIZED: Batch mark multiple notifications as read
        /// </summary>
        public async Task BatchMarkAsReadAsync(List<int> notificationIds, int userId)
        {
            if (notificationIds == null || notificationIds.Count == 0 || userId <= 0)
                return;

            var validIds = notificationIds.Where(id => id > 0).ToList();
            if (validIds.Count == 0)
                return;

            var now = DateTime.UtcNow;

            // OPTIMIZED: Single ExecuteUpdateAsync for all notifications
            await _context.Notifications
                .Where(x => validIds.Contains(x.Id) && x.ApplicationUserId == userId && !x.IsRead)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.IsRead, true)
                    .SetProperty(x => x.ReadDate, now)
                    .SetProperty(x => x.ChangeDate, now));
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            if (userId <= 0)
                return;

            var now = DateTime.UtcNow;

            // OPTIMIZED: Single ExecuteUpdateAsync instead of load-modify-save loop
            await _context.Notifications
                .Where(x => x.ApplicationUserId == userId && x.ViewState && !x.IsRead)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.IsRead, true)
                    .SetProperty(x => x.ReadDate, now)
                    .SetProperty(x => x.ChangeDate, now));
        }

        /// <summary>
        /// OPTIMIZED: Use ExecuteUpdateAsync instead of load-modify-save pattern
        /// </summary>
        public async Task DeleteAsync(int id, int userId)
        {
            if (id <= 0 || userId <= 0)
                return;

            var now = DateTime.UtcNow;

            await _context.Notifications
                .Where(x => x.Id == id && x.ApplicationUserId == userId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, now));
        }

        /// <summary>
        /// OPTIMIZED: Batch delete notifications
        /// </summary>
        public async Task BatchDeleteAsync(List<int> notificationIds, int userId)
        {
            if (notificationIds == null || notificationIds.Count == 0 || userId <= 0)
                return;

            var validIds = notificationIds.Where(id => id > 0).ToList();
            if (validIds.Count == 0)
                return;

            var now = DateTime.UtcNow;

            // OPTIMIZED: Single ExecuteUpdateAsync for all deletions
            await _context.Notifications
                .Where(x => validIds.Contains(x.Id) && x.ApplicationUserId == userId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, now));
        }

        /// <summary>
        /// OPTIMIZED: Delete all notifications for a user
        /// </summary>
        public async Task DeleteAllAsync(int userId)
        {
            if (userId <= 0)
                return;

            var now = DateTime.UtcNow;

            await _context.Notifications
                .Where(x => x.ApplicationUserId == userId && x.ViewState)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, now));
        }
    }
}
