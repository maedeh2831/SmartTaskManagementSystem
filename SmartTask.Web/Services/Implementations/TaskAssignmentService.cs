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
        public async Task RemoveUserAsync(int taskItemId, int userId)
        {
            var assignment = await _context.TaskAssignments
                .FirstOrDefaultAsync(x =>
                    x.TaskItemId == taskItemId &&
                    x.ApplicationUserId == userId &&
                    x.ViewState);
            if (assignment == null)
                return;
            assignment.ViewState = false;
            assignment.ChangeDate = DateTime.Now;
            await _context.SaveChangesAsync();

            var task = await _context.TaskItems.FirstOrDefaultAsync(x => x.Id == taskItemId);
            if (task != null)
            {
                await _activityLogService.LogAsync(
                    _currentUser.UserId,
                    "حذف تخصیص Task",
                    $"تخصیص Task «{task.Title}» حذف شد.",
                    taskItemId);
            }
        }

        private async Task NotifyAndLogAssignmentAsync(int taskItemId, int userId)
        {
            var task = await _context.TaskItems.FirstOrDefaultAsync(x => x.Id == taskItemId);
            if (task == null)
                return;

            await _notificationService.CreateAsync(
                userId,
                "تخصیص Task جدید",
                $"شما به Task «{task.Title}» تخصیص یافتید.",
                NotificationType.Assignment);

            var assignedUser = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            var assignedUserName = assignedUser != null
                ? $"{assignedUser.FirstName} {assignedUser.LastName}".Trim()
                : "یکی از اعضا";

            await _activityLogService.LogAsync(
                _currentUser.UserId,
                "تخصیص Task",
                $"Task «{task.Title}» به {assignedUserName} تخصیص یافت.",
                taskItemId);
        }
    }
}