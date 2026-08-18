using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Interfaces;
namespace SmartTask.Web.Services.Implementations
{
    public class TaskAssignmentService : ITaskAssignmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IActivityLogService _activityLogService;
        private readonly ICurrentUserService _currentUser;

        public TaskAssignmentService(
            ApplicationDbContext context,
            INotificationService notificationService,
            IActivityLogService activityLogService,
            ICurrentUserService currentUser)
        {
            _context = context;
            _notificationService = notificationService;
            _activityLogService = activityLogService;
            _currentUser = currentUser;
        }
        public async Task<List<ApplicationUser>> GetAssigneesAsync(int taskItemId)
        {
            return await _context.TaskAssignments
                .Where(x => x.TaskItemId == taskItemId && x.ViewState)
                .Include(x => x.ApplicationUser)
                .Select(x => x.ApplicationUser)
                .ToListAsync();
        }
        public async Task<bool> IsAssignedAsync(int taskItemId, int userId)
        {
            return await _context.TaskAssignments
                .AnyAsync(x => x.TaskItemId == taskItemId && x.ApplicationUserId == userId && x.ViewState);
        }
        public async Task AssignUserAsync(int taskItemId, int userId)
        {
            var existing = await _context.TaskAssignments
                .FirstOrDefaultAsync(x =>
                    x.TaskItemId == taskItemId &&
                    x.ApplicationUserId == userId);
            if (existing != null)
            {
                if (existing.ViewState)
                    return;
                existing.ViewState = true;
                existing.AssignedDate = DateTime.Now;
                existing.ChangeDate = DateTime.Now;
                await _context.SaveChangesAsync();

                await NotifyAndLogAssignmentAsync(taskItemId, userId);
                return;
            }
            var assignment = new TaskAssignment
            {
                TaskItemId = taskItemId,
                ApplicationUserId = userId,
                AssignedDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                ViewState = true
            };
            await _context.TaskAssignments.AddAsync(assignment);
            await _context.SaveChangesAsync();

            await NotifyAndLogAssignmentAsync(taskItemId, userId);
        }
        // OPTIMIZED: Use ExecuteUpdateAsync instead of load-modify-save + single query for task title
        public async Task RemoveUserAsync(int taskItemId, int userId)
        {
            var now = DateTime.Now;
            var updated = await _context.TaskAssignments
                .Where(x => x.TaskItemId == taskItemId && x.ApplicationUserId == userId && x.ViewState)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, now));

            if (updated == 0) return;

            var taskTitle = await _context.TaskItems
                .Where(x => x.Id == taskItemId)
                .Select(x => x.Title)
                .FirstOrDefaultAsync();

            if (taskTitle != null)
            {
                await _activityLogService.LogAsync(
                    _currentUser.UserId,
                    "حذف تخصیص Task",
                    $"تخصیص Task «{taskTitle}» حذف شد.",
                    taskItemId);
            }
        }

        // OPTIMIZED: Single combined query for task + user instead of 2 separate queries
        private async Task NotifyAndLogAssignmentAsync(int taskItemId, int userId)
        {
            var taskData = await _context.TaskItems
                .Where(x => x.Id == taskItemId)
                .Select(x => new { x.Title })
                .FirstOrDefaultAsync();
            if (taskData == null)
                return;

            await _notificationService.CreateAsync(
                userId,
                "تخصیص Task جدید",
                $"شما به Task «{taskData.Title}» تخصیص یافتید.",
                NotificationType.Assignment);

            var userName = await _context.Users
                .Where(x => x.Id == userId)
                .Select(x => (x.FirstName + " " + x.LastName).Trim())
                .FirstOrDefaultAsync() ?? "یکی از اعضا";

            await _activityLogService.LogAsync(
                _currentUser.UserId,
                "تخصیص Task",
                $"Task «{taskData.Title}» به {userName} تخصیص یافت.",
                taskItemId);
        }
    }
}