using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class SubTaskController : BaseController
{
    private readonly ISubTaskService _subTaskService;
    private readonly ITaskService _taskService;

    public SubTaskController(
        ISubTaskService subTaskService,
        ITaskService taskService,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _subTaskService = subTaskService;
        _taskService = taskService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickAdd(int taskItemId, string title)
    {
        if (!await _taskService.CanManageTaskAsync(taskItemId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه افزودن SubTask را ندارید.";
            return RedirectToAction("Details", "Task", new { id = taskItemId });
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            TempData["Error"] = "عنوان SubTask را وارد کنید.";
            return RedirectToAction("Details", "Task", new { id = taskItemId });
        }

        var subTask = new SubTaskItem
        {
            TaskItemId = taskItemId,
            Title = title.Trim(),
            IsCompleted = false,
            CreatedDate = DateTime.Now,
            ViewState = true
        };

        await _subTaskService.AddAsync(subTask);

        return RedirectToAction("Details", "Task", new { id = taskItemId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleComplete(int id, int taskItemId)
    {
        if (!await _subTaskService.CanManageSubTaskAsync(id, CurrentUser.UserId))
            return Forbid();

        await _subTaskService.ToggleCompleteAsync(id);
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int taskItemId)
    {
        if (!await _subTaskService.CanManageSubTaskAsync(id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه حذف این SubTask را ندارید.";
            return RedirectToAction("Details", "Task", new { id = taskItemId });
        }

        await _subTaskService.DeleteAsync(id);

        return RedirectToAction("Details", "Task", new { id = taskItemId });
    }
}