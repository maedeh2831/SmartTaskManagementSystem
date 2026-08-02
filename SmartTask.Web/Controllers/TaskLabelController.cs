using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class TaskLabelController : BaseController
{
    private readonly ITaskLabelService _taskLabelService;
    private readonly ITaskService _taskService;

    public TaskLabelController(
        ITaskLabelService taskLabelService,
        ITaskService taskService,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _taskLabelService = taskLabelService;
        _taskService = taskService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(int taskItemId, int labelId)
    {
        if (!await _taskService.CanManageTaskAsync(taskItemId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه افزودن Label به این Task را ندارید.";
            return RedirectToAction("Details", "Task", new { id = taskItemId });
        }

        await _taskLabelService.AssignLabelAsync(taskItemId, labelId);
        return RedirectToAction("Details", "Task", new { id = taskItemId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int taskItemId, int labelId)
    {
        if (!await _taskService.CanManageTaskAsync(taskItemId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه حذف Label را ندارید.";
            return RedirectToAction("Details", "Task", new { id = taskItemId });
        }

        await _taskLabelService.RemoveLabelAsync(taskItemId, labelId);
        return RedirectToAction("Details", "Task", new { id = taskItemId });
    }
}