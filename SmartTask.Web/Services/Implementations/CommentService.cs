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
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _activityLogService = activityLogService ?? throw new ArgumentNullException(nameof(activityLogService));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        }

        public async Task<List<Comment>> GetByTaskAsync(int taskItemId)
        {
            if (taskItemId <= 0)
                return new List<Comment>();

            return await _context.Comments
                .Where(x => x.TaskItemId == taskItemId && x.ViewState)
                .Include(x => x.ApplicationUser)
                .OrderBy(x => x.CreatedDate)
                .ToListAsync();
        }

        public async Task AddCommentAsync(int taskItemId, int userId, string content)
        {
            if (taskItemId <= 0 || userId <= 0 || string.IsNullOrWhiteSpace(content))
                return;

            var now = DateTime.Now;
            var trimmedContent = content.Trim();

            var comment = new Comment
            {
                TaskItemId = taskItemId,
                ApplicationUserId = userId,
                Content = trimmedContent,
                IsEdited = false,
                CreatedDate = now,
                ViewState = true
            };

            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            await NotifyAssigneesAsync(taskItemId, userId);
        }

        /// <summary>
        /// OPTIMIZED: Batch add comments in a single transaction
        /// </summary>
        public async Task BatchAddCommentsAsync(List<(int taskItemId, int userId, string content)> comments)
        {
            if (comments == null || comments.Count == 0)
                return;

            var validComments = comments
                .Where(x => x.taskItemId > 0 && x.userId > 0 && !string.IsNullOrWhiteSpace(x.content))
                .ToList();

            if (validComments.Count == 0)
                return;

            var now = DateTime.Now;
            var entities = validComments.Select(c => new Comment
            {
                TaskItemId = c.taskItemId,
                ApplicationUserId = c.userId,
                Content = c.content.Trim(),
                IsEdited = false,
                CreatedDate = now,
                ViewState = true
            }).ToList();

            // OPTIMIZED: Single SaveChangesAsync for all comments
            await _context.Comments.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CanEditCommentAsync(int commentId, int userId)
        {
            if (commentId <= 0 || userId <= 0)
                return false;

            return await _context.Comments
                .AnyAsync(x => x.Id == commentId && x.ApplicationUserId == userId && x.ViewState);
        }

        public async Task<bool> CanDeleteCommentAsync(int commentId, int userId)
        {
            if (commentId <= 0 || userId <= 0)
                return false;

            var comment = await _context.Comments
                .Where(x => x.Id == commentId && x.ViewState)
                .Select(x => new { x.ApplicationUserId, x.TaskItemId })
                .FirstOrDefaultAsync();

            if (comment == null)
                return false;

            // Owner can delete
            if (comment.ApplicationUserId == userId)
                return true;

            // Task manager can delete
            return await _taskService.CanManageTaskAsync(comment.TaskItemId, userId);
        }

        /// <summary>
        /// OPTIMIZED: Use ExecuteUpdateAsync instead of load-modify-save pattern
        /// </summary>
        public async Task EditCommentAsync(int commentId, string content)
        {
            if (commentId <= 0 || string.IsNullOrWhiteSpace(content))
                return;

            var trimmedContent = content.Trim();
            var now = DateTime.Now;

            var taskId = await _context.Comments
                .Where(x => x.Id == commentId && x.ViewState)
                .Select(x => x.TaskItemId)
                .FirstOrDefaultAsync();

            if (taskId <= 0)
                return;

            // OPTIMIZED: Single ExecuteUpdateAsync instead of load-modify-save
            await _context.Comments
                .Where(x => x.Id == commentId && x.ViewState)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.Content, trimmedContent)
                    .SetProperty(x => x.IsEdited, true)
                    .SetProperty(x => x.ChangeDate, now));

            await _activityLogService.LogAsync(_currentUser.UserId, "ویرایش نظر", null, taskId);
        }

        /// <summary>
        /// OPTIMIZED: Use ExecuteUpdateAsync instead of load-modify-save pattern
        /// </summary>
        public async Task DeleteCommentAsync(int commentId)
        {
            if (commentId <= 0)
                return;

            var now = DateTime.Now;

            var taskId = await _context.Comments
                .Where(x => x.Id == commentId && x.ViewState)
                .Select(x => x.TaskItemId)
                .FirstOrDefaultAsync();

            if (taskId <= 0)
                return;

            // OPTIMIZED: Single ExecuteUpdateAsync instead of load-modify-save
            await _context.Comments
                .Where(x => x.Id == commentId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, now));

            await _activityLogService.LogAsync(_currentUser.UserId, "حذف نظر", null, taskId);
        }

        /// <summary>
        /// OPTIMIZED: Batch delete comments
        /// </summary>
        public async Task BatchDeleteCommentsAsync(List<int> commentIds)
        {
            if (commentIds == null || commentIds.Count == 0)
                return;

            var validIds = commentIds.Where(id => id > 0).ToList();
            if (validIds.Count == 0)
                return;

            var now = DateTime.Now;

            // OPTIMIZED: Single ExecuteUpdateAsync for all deletions
            await _context.Comments
                .Where(x => validIds.Contains(x.Id))
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, now));
        }

        /// <summary>
        /// OPTIMIZED: Single query for task + commenter + assignees, then parallel notifications
        /// </summary>
        private async Task NotifyAssigneesAsync(int taskItemId, int commenterId)
        {
            if (taskItemId <= 0 || commenterId <= 0)
                return;

            // OPTIMIZED: Single combined query instead of 3 separate queries
            var taskData = await _context.TaskItems
                .Where(x => x.Id == taskItemId && x.ViewState)
                .Select(x => new
                {
                    x.Title,
                    AssigneeIds = x.Assignments
                        .Where(a => a.ViewState && a.ApplicationUserId != commenterId)
                        .Select(a => a.ApplicationUserId)
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (taskData == null || taskData.AssigneeIds.Count == 0)
                return;

            // Get commenter info
            var commenter = await _context.Users
                .Where(x => x.Id == commenterId)
                .Select(x => new { x.FirstName, x.LastName })
                .FirstOrDefaultAsync();

            var commenterName = commenter != null
                ? $"{commenter.FirstName} {commenter.LastName}".Trim()
                : "یکی از اعضا";

            // OPTIMIZED: Send notifications in parallel using Task.WhenAll
            var notificationTasks = taskData.AssigneeIds
                .Select(assigneeId =>
                    _notificationService.CreateAsync(
                        assigneeId,
                        "نظر جدید",
                        $"{commenterName} روی Task «{taskData.Title}» نظر جدیدی ثبت کرد.",
                        NotificationType.Comment))
                .ToList();

            await Task.WhenAll(notificationTasks);

            await _activityLogService.LogAsync(commenterId, "ثبت نظر", $"روی Task «{taskData.Title}» نظر ثبت شد.", taskItemId);
        }

        /// <summary>
        /// OPTIMIZED: Get comments count for multiple tasks
        /// </summary>
        public async Task<Dictionary<int, int>> GetCommentCountsAsync(List<int> taskItemIds)
        {
            if (taskItemIds == null || taskItemIds.Count == 0)
                return new Dictionary<int, int>();

            var validIds = taskItemIds.Where(id => id > 0).Distinct().ToList();
            if (validIds.Count == 0)
                return new Dictionary<int, int>();

            return await _context.Comments
                .Where(x => validIds.Contains(x.TaskItemId) && x.ViewState)
                .GroupBy(x => x.TaskItemId)
                .Select(g => new { TaskId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TaskId, x => x.Count);
        }

        /// <summary>
        /// OPTIMIZED: Get latest comments for multiple tasks
        /// </summary>
        public async Task<Dictionary<int, Comment?>> GetLatestCommentsAsync(List<int> taskItemIds)
        {
            if (taskItemIds == null || taskItemIds.Count == 0)
                return new Dictionary<int, Comment?>();

            var validIds = taskItemIds.Where(id => id > 0).Distinct().ToList();
            if (validIds.Count == 0)
                return new Dictionary<int, Comment?>();

            var comments = await _context.Comments
                .Where(x => validIds.Contains(x.TaskItemId) && x.ViewState)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            var result = new Dictionary<int, Comment?>();

            foreach (var taskId in validIds)
            {
                result[taskId] = comments
                    .Where(c => c.TaskItemId == taskId)
                    .FirstOrDefault();
            }

            return result;
        }
    }
}
