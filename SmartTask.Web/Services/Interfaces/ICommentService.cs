using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Services.Interfaces;

public interface ICommentService
{
    Task<List<Comment>> GetByTaskAsync(int taskItemId);
    Task AddCommentAsync(int taskItemId, int userId, string content);
    Task<bool> CanEditCommentAsync(int commentId, int userId);
    Task<bool> CanDeleteCommentAsync(int commentId, int userId);
    Task EditCommentAsync(int commentId, string content);
    Task DeleteCommentAsync(int commentId);
}