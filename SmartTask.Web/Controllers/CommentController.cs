using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class CommentController : BaseController
{
    private readonly ICommentService _commentService;

    public CommentController(ICommentService commentService, ICurrentUserService currentUser)
        : base(currentUser)
    {
        _commentService = commentService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int taskItemId, string content)
    {
        if (!string.IsNullOrWhiteSpace(content))
            await _commentService.AddCommentAsync(taskItemId, CurrentUser.UserId, content);

        return RedirectToAction("Details", "Task", new { id = taskItemId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int commentId, int taskItemId)
    {
        if (!await _commentService.CanDeleteCommentAsync(commentId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه حذف این نظر را ندارید.";
            return RedirectToAction("Details", "Task", new { id = taskItemId });
        }

        await _commentService.DeleteCommentAsync(commentId);
        return RedirectToAction("Details", "Task", new { id = taskItemId });
    }
}