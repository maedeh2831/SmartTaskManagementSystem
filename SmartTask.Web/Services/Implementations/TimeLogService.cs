using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class TimeLogService : ITimeLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITaskService _taskService;
        private readonly IActivityLogService _activityLogService;
        private readonly ICurrentUserService _currentUser;

        public TimeLogService(
            ApplicationDbContext context,
            ITaskService taskService,
            IActivityLogService activityLogService,
            ICurrentUserService currentUser)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
            _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<List<TimeLog>> GetByTaskAsync(int taskItemId)
        {
            if (taskItemId <= 0)
                return new List<TimeLog>();

            return await _context.TimeLogs
                .Where(x => x.TaskItemId == taskItemId && x.ViewState)
                .Include(x => x.ApplicationUser)
                .OrderByDescending(x => x.StartTime)
                .ToListAsync();
        }

        public async Task<TimeLog?> GetActiveTimerAsync(int taskItemId, int userId)
        {
            if (taskItemId <= 0 || userId <= 0)
                return null;

            return await _context.TimeLogs
                .FirstOrDefaultAsync(x =>
                    x.TaskItemId == taskItemId &&
                    x.ApplicationUserId == userId &&
                    x.EndTime == null &&
                    x.ViewState);
        }

        public async Task<TimeLog> StartTimerAsync(int taskItemId, int userId)
        {
            if (taskItemId <= 0 || userId <= 0)
                throw new ArgumentException("Invalid task or user ID");

            var existing = await GetActiveTimerAsync(taskItemId, userId);
            if (existing != null)
                return existing;

            var now = DateTime.Now;

            var timeLog = new TimeLog
            {
                TaskItemId = taskItemId,
                ApplicationUserId = userId,
                StartTime = now,
                EndTime = null,
                DurationMinutes = 0,
                CreatedDate = now,
                ViewState = true
            };

            await _context.TimeLogs.AddAsync(timeLog);
            await _context.SaveChangesAsync();

            await _activityLogService.LogAsync(userId, "شروع تایمر", null, taskItemId);

            return timeLog;
        }

        /// <summary>
        /// OPTIMIZED: Use ExecuteUpdateAsync instead of load-modify-save pattern
        /// </summary>
        public async Task StopTimerAsync(int timeLogId)
        {
            if (timeLogId <= 0)
                return;

            var now = DateTime.Now;

            // OPTIMIZED: Get current state before update
            var currentLog = await _context.TimeLogs
                .Where(x => x.Id == timeLogId && x.ViewState && x.EndTime == null)
                .Select(x => new { x.StartTime, x.TaskItemId })
                .FirstOrDefaultAsync();

            if (currentLog == null)
                return;

            var durationMinutes = (int)Math.Round((now - currentLog.StartTime).TotalMinutes);

            // OPTIMIZED: Single ExecuteUpdateAsync instead of load-modify-save
            await _context.TimeLogs
                .Where(x => x.Id == timeLogId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.EndTime, now)
                    .SetProperty(x => x.DurationMinutes, durationMinutes)
                    .SetProperty(x => x.ChangeDate, now));

            await _activityLogService.LogAsync(
                _currentUser.UserId,
                "توقف تایمر",
                $"{durationMinutes} دقیقه ثبت شد.",
                currentLog.TaskItemId);
        }

        public async Task AddManualLogAsync(int taskItemId, int userId, DateTime startTime, int durationMinutes, string? description)
        {
            if (taskItemId <= 0 || userId <= 0 || durationMinutes <= 0)
                return;

            var now = DateTime.Now;

            var timeLog = new TimeLog
            {
                TaskItemId = taskItemId,
                ApplicationUserId = userId,
                StartTime = startTime,
                EndTime = startTime.AddMinutes(durationMinutes),
                DurationMinutes = durationMinutes,
                Description = description,
                CreatedDate = now,
                ViewState = true
            };

            await _context.TimeLogs.AddAsync(timeLog);
            await _context.SaveChangesAsync();

            await _activityLogService.LogAsync(userId, "ثبت زمان دستی", $"{durationMinutes} دقیقه ثبت شد.", taskItemId);
        }

        /// <summary>
        /// OPTIMIZED: Batch add manual logs in a single transaction
        /// </summary>
        public async Task BatchAddManualLogsAsync(List<(int taskItemId, int userId, DateTime startTime, int durationMinutes, string? description)> logs)
        {
            if (logs == null || logs.Count == 0)
                return;

            var validLogs = logs
                .Where(x => x.taskItemId > 0 && x.userId > 0 && x.durationMinutes > 0)
                .ToList();

            if (validLogs.Count == 0)
                return;

            var now = DateTime.Now;
            var entities = validLogs.Select(log => new TimeLog
            {
                TaskItemId = log.taskItemId,
                ApplicationUserId = log.userId,
                StartTime = log.startTime,
                EndTime = log.startTime.AddMinutes(log.durationMinutes),
                DurationMinutes = log.durationMinutes,
                Description = log.description,
                CreatedDate = now,
                ViewState = true
            }).ToList();

            // OPTIMIZED: Single SaveChangesAsync for all logs
            await _context.TimeLogs.AddRangeAsync(entities);
            await _context.SaveChangesAsync();

            // Batch activity logs
            var activityLogs = validLogs.Select(log => (
                log.userId,
                "ثبت زمان دستی",
                (string?)$"{log.durationMinutes} دقیقه ثبت شد.",
                (int?)log.taskItemId
            )).ToList();

            await _activityLogService.BatchLogAsync(activityLogs);
        }

        public async Task<bool> CanManageLogAsync(int timeLogId, int userId)
        {
            if (timeLogId <= 0 || userId <= 0)
                return false;

            var ownerUserId = await _context.TimeLogs
                .Where(x => x.Id == timeLogId && x.ViewState)
                .Select(x => new { x.ApplicationUserId, x.TaskItemId })
                .FirstOrDefaultAsync();

            if (ownerUserId == null)
                return false;

            // User owns the log
            if (ownerUserId.ApplicationUserId == userId)
                return true;

            // User can manage the task
            return await _taskService.CanManageTaskAsync(ownerUserId.TaskItemId, userId);
        }

        /// <summary>
        /// OPTIMIZED: Use ExecuteUpdateAsync instead of load-modify-save pattern
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            if (id <= 0)
                return;

            var now = DateTime.Now;
            var taskId = await _context.TimeLogs
                .Where(x => x.Id == id && x.ViewState)
                .Select(x => x.TaskItemId)
                .FirstOrDefaultAsync();

            if (taskId <= 0)
                return;

            // OPTIMIZED: Single ExecuteUpdateAsync instead of load-modify-save
            await _context.TimeLogs
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, now));

            await _activityLogService.LogAsync(_currentUser.UserId, "حذف زمان ثبت‌شده", null, taskId);
        }

        /// <summary>
        /// OPTIMIZED: Batch delete time logs
        /// </summary>
        public async Task BatchDeleteAsync(List<int> ids)
        {
            if (ids == null || ids.Count == 0)
                return;

            var validIds = ids.Where(id => id > 0).ToList();
            if (validIds.Count == 0)
                return;

            var now = DateTime.Now;

            // OPTIMIZED: Single ExecuteUpdateAsync for all deletions
            await _context.TimeLogs
                .Where(x => validIds.Contains(x.Id))
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, now));
        }

        public async Task<int> GetTotalMinutesForTaskAsync(int taskItemId)
        {
            if (taskItemId <= 0)
                return 0;

            return await _context.TimeLogs
                .Where(x => x.TaskItemId == taskItemId && x.ViewState && x.EndTime != null)
                .SumAsync(x => x.DurationMinutes);
        }

        /// <summary>
        /// OPTIMIZED: Get total minutes for multiple tasks in a single query
        /// </summary>
        public async Task<Dictionary<int, int>> GetTotalMinutesForTasksAsync(List<int> taskItemIds)
        {
            if (taskItemIds == null || taskItemIds.Count == 0)
                return new Dictionary<int, int>();

            var validIds = taskItemIds.Where(id => id > 0).Distinct().ToList();
            if (validIds.Count == 0)
                return new Dictionary<int, int>();

            return await _context.TimeLogs
                .Where(x => validIds.Contains(x.TaskItemId) && x.ViewState && x.EndTime != null)
                .GroupBy(x => x.TaskItemId)
                .Select(g => new { TaskId = g.Key, TotalMinutes = g.Sum(x => x.DurationMinutes) })
                .ToDictionaryAsync(x => x.TaskId, x => x.TotalMinutes);
        }

        /// <summary>
        /// OPTIMIZED: Get total minutes for user across all tasks
        /// </summary>
        public async Task<int> GetTotalMinutesForUserAsync(int userId)
        {
            if (userId <= 0)
                return 0;

            return await _context.TimeLogs
                .Where(x => x.ApplicationUserId == userId && x.ViewState && x.EndTime != null)
                .SumAsync(x => x.DurationMinutes);
        }
    }
}
