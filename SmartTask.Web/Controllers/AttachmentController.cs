using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class AttachmentController : BaseController
{
    private readonly IAttachmentService _attachmentService;
    private readonly ITaskService _taskService;

    public AttachmentController(
        IAttachmentService attachmentService,
        ITaskService taskService,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _attachmentService = attachmentService;
        _taskService = taskService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(int taskItemId, IFormFile file)
    {
        if (!await _taskService.CanManageTaskAsync(taskItemId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه آپلود فایل روی این Task را ندارید.";
            return RedirectToAction("Details", "Task", new { id = taskItemId });
        }

        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "فایلی انتخاب نشده است.";
            return RedirectToAction("Details", "Task", new { id = taskItemId });
        }

        try
        {
            await _attachmentService.UploadAsync(taskItemId, CurrentUser.UserId, file);
            TempData["Success"] = "فایل با موفقیت آپلود شد.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Details", "Task", new { id = taskItemId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int taskItemId)
    {
        if (!await _attachmentService.CanDeleteAttachmentAsync(id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه حذف این فایل را ندارید.";
            return RedirectToAction("Details", "Task", new { id = taskItemId });
        }

        await _attachmentService.DeleteAsync(id);

        TempData["Success"] = "فایل حذف شد.";
        return RedirectToAction("Details", "Task", new { id = taskItemId });
    }
}