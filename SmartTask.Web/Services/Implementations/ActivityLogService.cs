using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class ActivityLogService : IActivityLogService
    {
        private readonly ApplicationDbContext _context;

        public ActivityLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(int userId, string action, string? description = null, int? taskItemId = null)
        {
            var log = new ActivityLog
            {
                ApplicationUserId = userId,
                TaskItemId = taskItemId,
                Action = action,
                Description = description,
                ActivityDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                ViewState = true
            };

            await _context.ActivityLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ActivityLog>> GetUserActivitiesAsync(int userId, int take = 50)
        {
            return await _context.ActivityLogs
                .Where(x => x.ApplicationUserId == userId && x.ViewState)
                .Include(x => x.TaskItem)
                .OrderByDescending(x => x.ActivityDate)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<ActivityLog>> GetTaskActivitiesAsync(int taskItemId)
        {
            return await _context.ActivityLogs
                .Where(x => x.TaskItemId == taskItemId && x.ViewState)
                .Include(x => x.ApplicationUser)
                .OrderByDescending(x => x.ActivityDate)
                .ToListAsync();
        }
    }
}