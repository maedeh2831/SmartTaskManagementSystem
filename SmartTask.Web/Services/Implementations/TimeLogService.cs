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
            _context = context;
            _taskService = taskService;
            _activityLogService = activityLogService;
            _currentUser = currentUser;
        }

        public async Task<List<TimeLog>> GetByTaskAsync(int taskItemId)
        {
            return await _context.TimeLogs
                .Where(x => x.TaskItemId == taskItemId && x.ViewState)
                .Include(x => x.ApplicationUser)
                .OrderByDescending(x => x.StartTime)
                .ToListAsync();
        }

        public async Task<TimeLog?> GetActiveTimerAsync(int taskItemId, int userId)
        {
            return await _context.TimeLogs
                .FirstOrDefaultAsync(x =>
                    x.TaskItemId == taskItemId &&
                    x.ApplicationUserId == userId &&
                    x.EndTime == null &&
                    x.ViewState);
        }

        public async Task<TimeLog> StartTimerAsync(int taskItemId, int userId)
        {
            var existing = await GetActiveTimerAsync(taskItemId, userId);
            if (existing != null)
                return existing;

            var timeLog = new TimeLog
            {
                TaskItemId = taskItemId,
                ApplicationUserId = userId,
                StartTime = DateTime.Now,
                EndTime = null,
                DurationMinutes = 0,
                CreatedDate = DateTime.Now,
                ViewState = true
            };

            await _context.TimeLogs.AddAsync(timeLog);
            await _context.SaveChangesAsync();

            await _activityLogService.LogAsync(userId, "شروع تایمر", null, taskItemId);

            return timeLog;
        }

        public async Task StopTimerAsync(int timeLogId)
        {
            var timeLog = await _context.TimeLogs.FirstOrDefaultAsync(x => x.Id == timeLogId);
            if (timeLog == null || timeLog.EndTime.HasValue)
                return;

            timeLog.EndTime = DateTime.Now;
            timeLog.DurationMinutes = (int)Math.Round((timeLog.EndTime.Value - timeLog.StartTime).TotalMinutes);
            timeLog.ChangeDate = DateTime.Now;

            await _context.SaveChangesAsync();

            await _activityLogService.LogAsync(
                _currentUser.UserId,
                "توقف تایمر",
                $"{timeLog.DurationMinutes} دقیقه ثبت شد.",
                timeLog.TaskItemId);
        }

        public async Task AddManualLogAsync(int taskItemId, int userId, DateTime startTime, int durationMinutes, string? description)
        {
            var timeLog = new TimeLog
            {
                TaskItemId = taskItemId,
                ApplicationUserId = userId,
                StartTime = startTime,
                EndTime = startTime.AddMinutes(durationMinutes),
                DurationMinutes = durationMinutes,
                Description = description,
                CreatedDate = DateTime.Now,
                ViewState = true
            };

            await _context.TimeLogs.AddAsync(timeLog);
            await _context.SaveChangesAsync();

            await _activityLogService.LogAsync(userId, "ثبت زمان دستی", $"{durationMinutes} دقیقه ثبت شد.", taskItemId);
        }

        public async Task<bool> CanManageLogAsync(int timeLogId, int userId)
        {
            var log = await _context.TimeLogs.FirstOrDefaultAsync(x => x.Id == timeLogId);
            if (log == null) return false;

            if (log.ApplicationUserId == userId)
                return true;

            return await _taskService.CanManageTaskAsync(log.TaskItemId, userId);
        }

        public async Task DeleteAsync(int id)
        {
            var log = await _context.TimeLogs.FirstOrDefaultAsync(x => x.Id == id);
            if (log == null) return;

            log.ViewState = false;
            await _context.SaveChangesAsync();

            await _activityLogService.LogAsync(_currentUser.UserId, "حذف زمان ثبت‌شده", null, log.TaskItemId);
        }

        public async Task<int> GetTotalMinutesForTaskAsync(int taskItemId)
        {
            return await _context.TimeLogs
                .Where(x => x.TaskItemId == taskItemId && x.ViewState && x.EndTime != null)
                .SumAsync(x => x.DurationMinutes);
        }
    }
}