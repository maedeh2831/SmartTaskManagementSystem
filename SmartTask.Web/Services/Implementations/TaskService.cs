using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Gamification;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class TaskService : BaseService<TaskItem>, ITaskService
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserStoryService _userStoryService;
        private readonly INotificationService _notificationService;
        private readonly IActivityLogService _activityLogService;
        private readonly ICurrentUserService _currentUser;
        private readonly ITaskRewardCoordinator _rewardCoordinator;

        public TaskService(
            IGenericRepository<TaskItem> repository,
            IUnitOfWork unitOfWork,
            ApplicationDbContext context,
            IUserStoryService userStoryService,
            INotificationService notificationService,
            IActivityLogService activityLogService,
            ICurrentUserService currentUser,
            ITaskRewardCoordinator rewardCoordinator)
            : base(repository, unitOfWork)
        {
            _context = context;
            _userStoryService = userStoryService;
            _notificationService = notificationService;
            _activityLogService = activityLogService;
            _currentUser = currentUser;
            _rewardCoordinator = rewardCoordinator;
        }

        public override async Task AddAsync(TaskItem entity)
        {
            await base.AddAsync(entity);
            await _activityLogService.LogAsync(_currentUser.UserId, "ایجاد Task", $"Task «{entity.Title}» ایجاد شد.", entity.Id);
        }

        public override async Task UpdateAsync(TaskItem entity)
        {
            await base.UpdateAsync(entity);
            await _activityLogService.LogAsync(_currentUser.UserId, "ویرایش Task", $"Task «{entity.Title}» ویرایش شد.", entity.Id);
        }

        public async Task<TaskItem?> GetDetailsAsync(int id)
        {
            return await _context.TaskItems
                .Include(x => x.UserStory)
                    .ThenInclude(x => x.Project)
                .FirstOrDefaultAsync(x => x.Id == id && x.ViewState);
        }

        public async Task<List<TaskItem>> GetByUserStoryAsync(int userStoryId)
        {
            return await _context.TaskItems
                .Where(x => x.UserStoryId == userStoryId && x.ViewState)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> ExistsByTitleAsync(int userStoryId, string title, int? excludeId = null)
        {
            var query = _repository.Query()
                .Where(x => x.UserStoryId == userStoryId && x.Title == title && x.ViewState);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> CanManageTaskAsync(int taskId, int userId)
        {
            var userStoryId = await _repository.Query()
                .Where(x => x.Id == taskId)
                .Select(x => x.UserStoryId)
                .FirstOrDefaultAsync();

            if (userStoryId == 0)
                return false;

            return await _userStoryService.CanManageStoryAsync(userStoryId, userId);
        }

        public async Task ChangeStatusAsync(int taskId, TaskStatusType status)
        {
            var task = await _context.TaskItems
                .Include(x => x.Assignments.Where(a => a.ViewState))
                    .ThenInclude(a => a.ApplicationUser)
                .FirstOrDefaultAsync(x => x.Id == taskId);

            if (task == null)
                return;

            var wasAlreadyDone = task.Status == TaskStatusType.Done;

            task.Status = status;
            task.ChangeDate = DateTime.Now;

            if (status == TaskStatusType.Done)
                task.CompletedDate = DateTime.Now;
            else if (task.CompletedDate.HasValue)
                task.CompletedDate = null;

            await _context.SaveChangesAsync();

            var statusDisplay = GetStatusDisplay(status);

            await _activityLogService.LogAsync(
                _currentUser.UserId,
                "تغییر وضعیت Task",
                $"وضعیت Task «{task.Title}» به «{statusDisplay}» تغییر کرد.",
                task.Id);

            await NotifyStatusChangeAsync(task, statusDisplay);

            // اعطای پاداش گیمیفیکیشن فقط در اولین تکمیل تسک
            if (status == TaskStatusType.Done && !wasAlreadyDone)
            {
                var assigneeIds = task.Assignments
                    .Where(a => a.ViewState)
                    .Select(a => a.ApplicationUserId)
                    .ToList();

                await _rewardCoordinator.HandleTaskCompletedAsync(
                    task.Id,
                    task.Title,
                    assigneeIds,
                    task.Priority,
                    task.Estimate);
            }
        }

        private static string GetStatusDisplay(TaskStatusType status) => status switch
        {
            TaskStatusType.ToDo => "برای انجام",
            TaskStatusType.InProgress => "درحال انجام",
            TaskStatusType.InReview => "درحال بررسی",
            TaskStatusType.Done => "انجام‌شده",
            TaskStatusType.Cancelled => "لغو‌شده",
            _ => status.ToString()
        };

        private async Task NotifyStatusChangeAsync(TaskItem task, string statusDisplay)
        {
            var assigneeIds = task.Assignments
                .Where(a => a.ViewState)
                .Select(a => a.ApplicationUserId)
                .ToList();

            if (assigneeIds.Count == 0)
                return;

            var tasks = assigneeIds.Select(assigneeId =>
                _notificationService.CreateAsync(
                    assigneeId,
                    "تغییر وضعیت Task",
                    $"وضعیت Task «{task.Title}» به «{statusDisplay}» تغییر کرد.",
                    NotificationType.StatusChange));

            await Task.WhenAll(tasks);
        }

        public new async Task DeleteAsync(int id)
        {
            var task = await _context.TaskItems
                .FirstOrDefaultAsync(x => x.Id == id);

            if (task == null)
                return;

            task.ViewState = false;
            await _context.SaveChangesAsync();
            await _activityLogService.LogAsync(_currentUser.UserId, "حذف Task", $"Task «{task.Title}» حذف شد.", task.Id);
        }

        public async Task<List<TaskItem>> GetProjectBoardAsync(
            int projectId,
            int? assigneeId = null,
            TaskPriorityType? priority = null,
            TaskType? type = null,
            int? labelId = null)
        {
            var query = _context.TaskItems
                .Where(x => x.ViewState && x.UserStory.ViewState && x.UserStory.ProjectId == projectId);

            // Apply filters before includes to reduce cartesian products
            if (assigneeId.HasValue)
                query = query.Where(x => x.Assignments.Any(a => a.ViewState && a.ApplicationUserId == assigneeId.Value));

            if (priority.HasValue)
                query = query.Where(x => x.Priority == priority.Value);

            if (type.HasValue)
                query = query.Where(x => x.Type == type.Value);

            if (labelId.HasValue)
                query = query.Where(x => x.TaskLabels.Any(tl => tl.ViewState && tl.LabelId == labelId.Value));

            return await query
                .Include(x => x.UserStory)
                .Include(x => x.Assignments.Where(a => a.ViewState))
                    .ThenInclude(a => a.ApplicationUser)
                .Include(x => x.TaskLabels.Where(tl => tl.ViewState))
                    .ThenInclude(tl => tl.Label)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();
        }
    }
}
