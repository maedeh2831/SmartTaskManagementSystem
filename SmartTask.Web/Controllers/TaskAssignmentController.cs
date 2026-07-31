using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class TaskAssignmentController : BaseController
{
    private readonly ITaskAssignmentService _taskAssignmentService;
    private readonly ITaskService _taskService;

    public TaskAssignmentController(
        ITaskAssignmentService taskAssignmentService,
        ITaskService taskService,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _taskAssignmentService = taskAssignmentService;
        _taskService = taskService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignMember(int taskItemId, int userId)
    {
        if (!await _taskService.CanManageTaskAsync(taskItemId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه تخصیص عضو به این Task را ندارید.";
            return RedirectToAction("Details", "Task", new { id = taskItemId });
        }

        await _taskAssignmentService.AssignUserAsync(taskItemId, userId);

        TempData["Success"] = "عضو با موفقیت تخصیص یافت.";
        return RedirectToAction("Details", "Task", new { id = taskItemId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(int taskItemId, int userId)
    {
        if (!await _taskService.CanManageTaskAsync(taskItemId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه حذف این تخصیص را ندارید.";
            return RedirectToAction("Details", "Task", new { id = taskItemId });
        }

        await _taskAssignmentService.RemoveUserAsync(taskItemId, userId);

        TempData["Success"] = "تخصیص حذف شد.";
        return RedirectToAction("Details", "Task", new { id = taskItemId });
    }
}