using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class WorkloadController : BaseController
{
    private readonly IWorkloadAnalysisService _workloadAnalysisService;
    private readonly IProjectMemberService _projectMemberService;

    public WorkloadController(
        IWorkloadAnalysisService workloadAnalysisService,
        IProjectMemberService projectMemberService,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _workloadAnalysisService = workloadAnalysisService;
        _projectMemberService = projectMemberService;
    }

    public async Task<IActionResult> Index(int projectId)
    {
        if (!await _projectMemberService.IsMemberAsync(projectId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما عضو این پروژه نیستید.";
            return RedirectToAction("Index", "Project");
        }

        var vm = await _workloadAnalysisService.GetWorkloadAsync(projectId, CurrentUser.UserId);

        if (vm == null)
            return NotFound();

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCapacity(int projectMemberId, int projectId, int weeklyCapacityHours)
    {
        if (!await _projectMemberService.IsMemberAsync(projectId, CurrentUser.UserId))
            return Forbid();

        var canManage = await Request.HttpContext.RequestServices
            .GetRequiredService<IProjectService>()
            .CanManageProjectAsync(projectId, CurrentUser.UserId);

        if (!canManage)
            return Forbid();

        await _workloadAnalysisService.UpdateCapacityAsync(projectMemberId, weeklyCapacityHours);
        return Json(new { success = true });
    }
}