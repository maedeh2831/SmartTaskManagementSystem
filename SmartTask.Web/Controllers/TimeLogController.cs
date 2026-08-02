using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class TimeLogController : BaseController
{
    private readonly ITimeLogService _timeLogService;
    private readonly ITaskService _taskService;

    public TimeLogController(
        ITimeLogService timeLogService,
        ITaskService taskService,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _timeLogService = timeLogService;
        _taskService = taskService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int taskItemId)
    {
        await _timeLogService.StartTimerAsync(taskItemId, CurrentUser.UserId);
        return RedirectToAction("Details", "Task", new { id = taskItemId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Stop(int id, int taskItemId)
    {
        if (!await _timeLogService.CanManageLogAsync(id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه توقف این تایمر را ندارید.";
            return RedirectToAction("Details", "Task", new { id = taskItemId });
        }

        await _timeLogService.StopTimerAsync(id);
        return RedirectToAction("Details", "Task", new { id = taskItemId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddManual(int taskItemId, DateTime startTime, int durationMinutes, string? description)
    {
        if (!await _taskService.CanManageTaskAsync(taskItemId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ثبت زمان روی این Task را ندارید.";
            return RedirectToAction("Details", "Task", new { id = taskItemId });
        }

        if (durationMinutes <= 0)
        {
            TempData["Error"] = "مدت زمان باید بیشتر از صفر باشد.";
            return RedirectToAction("Details", "Task", new { id = taskItemId });
        }

        await _timeLogService.AddManualLogAsync(taskItemId, CurrentUser.UserId, startTime, durationMinutes, description);

        TempData["Success"] = "زمان با موفقیت ثبت شد.";
        return RedirectToAction("Details", "Task", new { id = taskItemId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int taskItemId)
    {
        if (!await _timeLogService.CanManageLogAsync(id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه حذف این رکورد زمانی را ندارید.";
            return RedirectToAction("Details", "Task", new { id = taskItemId });
        }

        await _timeLogService.DeleteAsync(id);
        return RedirectToAction("Details", "Task", new { id = taskItemId });
    }
}