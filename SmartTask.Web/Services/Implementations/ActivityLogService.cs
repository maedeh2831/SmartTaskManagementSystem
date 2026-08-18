using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly ApplicationDbContext _context;
        private const int DefaultTakeCount = 50;

        public ActivityLogService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task LogAsync(int userId, string action, string? description = null, int? taskItemId = null)
        {
            // Validate inputs
            if (userId <= 0 || string.IsNullOrWhiteSpace(action))
                return;

            var now = DateTime.Now;

            var log = new ActivityLog
            {
                ApplicationUserId = userId,
                TaskItemId = taskItemId,
                Action = action,
                Description = description,
                ActivityDate = now,
                CreatedDate = now,
                ViewState = true
            };

            await _context.ActivityLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// OPTIMIZED: Batch log multiple activities in a single SaveChangesAsync call
        /// </summary>
        public async Task BatchLogAsync(List<(int userId, string action, string? description, int? taskItemId)> logs)
        {
            if (logs == null || logs.Count == 0)
                return;

            var now = DateTime.Now;
            var validLogs = logs
                .Where(x => x.userId > 0 && !string.IsNullOrWhiteSpace(x.action))
                .ToList();

            if (validLogs.Count == 0)
                return;

            var entities = validLogs.Select(log => new ActivityLog
            {
                ApplicationUserId = log.userId,
                TaskItemId = log.taskItemId,
                Action = log.action,
                Description = log.description,
                ActivityDate = now,
                CreatedDate = now,
                ViewState = true
            }).ToList();

            // OPTIMIZED: Single SaveChangesAsync for all logs
            await _context.ActivityLogs.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ActivityLog>> GetUserActivitiesAsync(int userId, int take = DefaultTakeCount)
        {
            if (userId <= 0)
                return new List<ActivityLog>();

            return await _context.ActivityLogs
                .Where(x => x.ApplicationUserId == userId && x.ViewState)
                .Include(x => x.TaskItem)
                .OrderByDescending(x => x.ActivityDate)
                .Take(Math.Max(1, take))
                .ToListAsync();
        }

        public async Task<List<ActivityLog>> GetTaskActivitiesAsync(int taskItemId)
        {
            if (taskItemId <= 0)
                return new List<ActivityLog>();

            return await _context.ActivityLogs
                .Where(x => x.TaskItemId == taskItemId && x.ViewState)
                .Include(x => x.ApplicationUser)
                .OrderByDescending(x => x.ActivityDate)
                .ToListAsync();
        }

        /// <summary>
        /// OPTIMIZED: Get recent activities for multiple users in parallel
        /// </summary>
        public async Task<Dictionary<int, List<ActivityLog>>> GetMultipleUserActivitiesAsync(List<int> userIds, int take = DefaultTakeCount)
        {
            if (userIds == null || userIds.Count == 0)
                return new Dictionary<int, List<ActivityLog>>();

            var validIds = userIds.Where(id => id > 0).Distinct().ToList();
            if (validIds.Count == 0)
                return new Dictionary<int, List<ActivityLog>>();

            var activities = await _context.ActivityLogs
                .Where(x => validIds.Contains(x.ApplicationUserId) && x.ViewState)
                .Include(x => x.TaskItem)
                .OrderByDescending(x => x.ActivityDate)
                .ToListAsync();

            // Group in-memory to avoid multiple queries
            return activities
                .GroupBy(x => x.ApplicationUserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Take(take).ToList());
        }
    }
}
