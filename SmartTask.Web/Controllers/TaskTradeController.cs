using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class TaskTradeController : BaseController
{
    private readonly ITaskTradeService _tradeService;
    private readonly IProjectMemberService _projectMemberService;

    public TaskTradeController(
        ITaskTradeService tradeService,
        IProjectMemberService projectMemberService,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _tradeService = tradeService;
        _projectMemberService = projectMemberService;
    }

    public async Task<IActionResult> Index(int projectId)
    {
        if (!await _projectMemberService.IsMemberAsync(projectId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما عضو این پروژه نیستید.";
            return RedirectToAction("Index", "Project");
        }

        var vm = await _tradeService.GetProjectRequestsAsync(projectId, CurrentUser.UserId);
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserTasks(int projectId, int userId, int excludeTaskId)
    {
        var tasks = await _tradeService.GetUserTasksAsync(projectId, userId, excludeTaskId);
        var result = tasks.Select(t => new { id = int.Parse(t.Value!), title = t.Text });
        return Json(new { success = true, tasks = result });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int taskId, int projectId, int targetUserId, int? targetTaskId, string? message)
    {
        var (success, error) = await _tradeService.CreateRequestAsync(
            projectId, CurrentUser.UserId, taskId, targetUserId, targetTaskId, message);

        TempData[success ? "Success" : "Error"] = success ? "درخواست ترید با موفقیت ارسال شد." : error;
        return RedirectToAction("Details", "Task", new { id = taskId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(int id, int projectId)
    {
        var (success, error) = await _tradeService.AcceptAsync(id, CurrentUser.UserId);
        TempData[success ? "Success" : "Error"] = success ? "ترید با موفقیت انجام شد." : error;
        return RedirectToAction(nameof(Index), new { projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, int projectId)
    {
        var (success, error) = await _tradeService.RejectAsync(id, CurrentUser.UserId);
        TempData[success ? "Success" : "Error"] = success ? "درخواست رد شد." : error;
        return RedirectToAction(nameof(Index), new { projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, int projectId)
    {
        var (success, error) = await _tradeService.CancelAsync(id, CurrentUser.UserId);
        TempData[success ? "Success" : "Error"] = success ? "درخواست لغو شد." : error;
        return RedirectToAction(nameof(Index), new { projectId });
    }
}