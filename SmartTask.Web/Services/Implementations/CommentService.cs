using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Interfaces;
namespace SmartTask.Web.Services.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITaskService _taskService;
        private readonly INotificationService _notificationService;
        private readonly IActivityLogService _activityLogService;
        private readonly ICurrentUserService _currentUser;

        public CommentService(
            ApplicationDbContext context,
            ITaskService taskService,
            INotificationService notificationService,
            IActivityLogService activityLogService,
            ICurrentUserService currentUser)
        {
            _context = context;
            _taskService = taskService;
            _notificationService = notificationService;
            _activityLogService = activityLogService;
            _currentUser = currentUser;
        }
        public async Task<List<Comment>> GetByTaskAsync(int taskItemId)
        {
            return await _context.Comments
                .Where(x => x.TaskItemId == taskItemId && x.ViewState)
                .Include(x => x.ApplicationUser)
                .OrderBy(x => x.CreatedDate)
                .ToListAsync();
        }
        public async Task AddCommentAsync(int taskItemId, int userId, string content)
        {
            var comment = new Comment
            {
                TaskItemId = taskItemId,
                ApplicationUserId = userId,
                Content = content.Trim(),
                IsEdited = false,
                CreatedDate = DateTime.Now,
                ViewState = true
            };
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            await NotifyAssigneesAsync(taskItemId, userId);
        }
        public async Task<bool> CanEditCommentAsync(int commentId, int userId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(x => x.Id == commentId);
            return comment != null && comment.ApplicationUserId == userId;
        }
        public async Task<bool> CanDeleteCommentAsync(int commentId, int userId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(x => x.Id == commentId);
            if (comment == null) return false;
            if (comment.ApplicationUserId == userId)
                return true;
            return await _taskService.CanManageTaskAsync(comment.TaskItemId, userId);
        }
        public async Task EditCommentAsync(int commentId, string content)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(x => x.Id == commentId);
            if (comment == null) return;
            comment.Content = content.Trim();
            comment.IsEdited = true;
            comment.ChangeDate = DateTime.Now;
            await _context.SaveChangesAsync();

            await _activityLogService.LogAsync(_currentUser.UserId, "ویرایش نظر", null, comment.TaskItemId);
        }
        public async Task DeleteCommentAsync(int commentId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(x => x.Id == commentId);
            if (comment == null) return;
            comment.ViewState = false;
            await _context.SaveChangesAsync();

            await _activityLogService.LogAsync(_currentUser.UserId, "حذف نظر", null, comment.TaskItemId);
        }

        private async Task NotifyAssigneesAsync(int taskItemId, int commenterId)
        {
            var task = await _context.TaskItems.FirstOrDefaultAsync(x => x.Id == taskItemId);
            if (task == null)
                return;

            var commenter = await _context.Users.FirstOrDefaultAsync(x => x.Id == commenterId);
            var commenterName = commenter != null
                ? $"{commenter.FirstName} {commenter.LastName}".Trim()
                : "یکی از اعضا";

            var assigneeIds = await _context.TaskAssignments
                .Where(x => x.TaskItemId == taskItemId && x.ViewState && x.ApplicationUserId != commenterId)
                .Select(x => x.ApplicationUserId)
                .ToListAsync();

            foreach (var assigneeId in assigneeIds)
            {
                await _notificationService.CreateAsync(
                    assigneeId,
                    "نظر جدید",
                    $"{commenterName} روی Task «{task.Title}» نظر جدیدی ثبت کرد.",
                    NotificationType.Comment);
            }

            await _activityLogService.LogAsync(commenterId, "ثبت نظر", $"روی Task «{task.Title}» نظر ثبت شد.", taskItemId);
        }
    }
}