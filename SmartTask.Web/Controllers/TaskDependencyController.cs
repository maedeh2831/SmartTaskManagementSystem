using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class TaskDependencyController : BaseController
{
    private readonly ITaskDependencyService _dependencyService;
    private readonly ITaskService _taskService;

    public TaskDependencyController(
        ITaskDependencyService dependencyService,
        ITaskService taskService,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _dependencyService = dependencyService;
        _taskService = taskService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int taskItemId, int dependsOnTaskId, bool isRequired)
    {
        if (!await _taskService.CanManageTaskAsync(taskItemId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه مدیریت وابستگی‌های این Task را ندارید.";
            return RedirectToAction("Details", "Task", new { id = taskItemId });
        }

        var (success, error) = await _dependencyService.AddDependencyAsync(taskItemId, dependsOnTaskId, isRequired);

        TempData[success ? "Success" : "Error"] = success ? "وابستگی با موفقیت ثبت شد." : error;
        return RedirectToAction("Details", "Task", new { id = taskItemId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int id, int taskItemId)
    {
        if (!await _taskService.CanManageTaskAsync(taskItemId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه مدیریت وابستگی‌های این Task را ندارید.";
            return RedirectToAction("Details", "Task", new { id = taskItemId });
        }

        await _dependencyService.RemoveDependencyAsync(id);

        TempData["Success"] = "وابستگی حذف شد.";
        return RedirectToAction("Details", "Task", new { id = taskItemId });
    }
}