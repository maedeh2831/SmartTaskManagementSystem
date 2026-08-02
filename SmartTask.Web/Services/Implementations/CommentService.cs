using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITaskService _taskService;

        public CommentService(ApplicationDbContext context, ITaskService taskService)
        {
            _context = context;
            _taskService = taskService;
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
        }

        public async Task DeleteCommentAsync(int commentId)
        {
            var comment = await _context.Comments.FirstOrDefaultAsync(x => x.Id == commentId);
            if (comment == null) return;

            comment.ViewState = false;
            await _context.SaveChangesAsync();
        }
    }
}