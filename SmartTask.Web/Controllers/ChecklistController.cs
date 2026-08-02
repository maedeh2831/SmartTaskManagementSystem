using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class ChecklistController : BaseController
{
    private readonly IChecklistService _checklistService;
    private readonly ITaskService _taskService;

    public ChecklistController(
        IChecklistService checklistService,
        ITaskService taskService,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _checklistService = checklistService;
        _taskService = taskService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int taskItemId, string title)
    {
        if (!await _taskService.CanManageTaskAsync(taskItemId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ساخت Checklist را ندارید.";
            return RedirectToAction("Details", "Task", new { id = taskItemId });
        }

        if (!string.IsNullOrWhiteSpace(title))
            await _checklistService.CreateChecklistAsync(taskItemId, title);

        return RedirectToAction("Details", "Task", new { id = taskItemId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int taskItemId)
    {
        if (!await _checklistService.CanManageChecklistAsync(id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه حذف این Checklist را ندارید.";
            return RedirectToAction("Details", "Task", new { id = taskItemId });
        }

        await _checklistService.DeleteChecklistAsync(id);
        return RedirectToAction("Details", "Task", new { id = taskItemId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddItem(int checklistId, string title, int taskItemId)
    {
        if (!await _checklistService.CanManageChecklistAsync(checklistId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه افزودن آیتم را ندارید.";
            return RedirectToAction("Details", "Task", new { id = taskItemId });
        }

        if (!string.IsNullOrWhiteSpace(title))
            await _checklistService.AddItemAsync(checklistId, title);

        return RedirectToAction("Details", "Task", new { id = taskItemId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleItem(int id)
    {
        if (!await _checklistService.CanManageItemAsync(id, CurrentUser.UserId))
            return Forbid();

        await _checklistService.ToggleItemAsync(id);
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteItem(int id, int taskItemId)
    {
        if (!await _checklistService.CanManageItemAsync(id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه حذف این آیتم را ندارید.";
            return RedirectToAction("Details", "Task", new { id = taskItemId });
        }

        await _checklistService.DeleteItemAsync(id);
        return RedirectToAction("Details", "Task", new { id = taskItemId });
    }
}