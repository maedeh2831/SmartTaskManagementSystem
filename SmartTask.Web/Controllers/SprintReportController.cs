using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class SprintReportController : BaseController
{
    private readonly ISprintReportAiService _sprintReportAiService;
    private readonly ISprintService _sprintService;
    private readonly IProjectMemberService _projectMemberService;

    public SprintReportController(
        ISprintReportAiService sprintReportAiService,
        ISprintService sprintService,
        IProjectMemberService projectMemberService,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _sprintReportAiService = sprintReportAiService;
        _sprintService = sprintService;
        _projectMemberService = projectMemberService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int projectId)
    {
        if (!await _projectMemberService.IsMemberAsync(projectId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما عضو این پروژه نیستید.";
            return RedirectToAction("Index", "Project");
        }

        var sprints = await _sprintService.GetByProjectAsync(projectId);
        ViewBag.ProjectId = projectId;
        return View(sprints);
    }

    [HttpGet]
    public async Task<IActionResult> GetReports(int sprintId)
    {
        var sprint = await _sprintService.GetByIdAsync(sprintId);
        if (sprint == null)
            return NotFound();

        if (!await _sprintService.CanManageSprintAsync(sprintId, CurrentUser.UserId))
            return Forbid();

        var reports = await _sprintReportAiService.GetReportsAsync(sprintId);
        return Json(new { success = true, reports });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(int sprintId)
    {
        var sprint = await _sprintService.GetByIdAsync(sprintId);
        if (sprint == null)
            return Json(new { success = false, message = "اسپرینت یافت نشد (id=" + sprintId + ")" });

        var canManage = await _sprintService.CanManageSprintAsync(sprintId, CurrentUser.UserId);
        if (!canManage)
            return Json(new { success = false, message = "شما اجازه مدیریت این اسپرینت را ندارید. (userId=" + CurrentUser.UserId + ", sprintId=" + sprintId + ")" });

        try
        {
            var report = await _sprintReportAiService.GenerateReportAsync(sprintId, CurrentUser.UserId);
            return Json(new { success = true, report });
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException != null ? " | Inner: " + ex.InnerException.Message : "";
            return Json(new { success = false, message = "خطا: " + ex.Message + inner });
        }
    }
}