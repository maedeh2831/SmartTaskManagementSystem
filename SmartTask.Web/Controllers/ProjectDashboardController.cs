using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class ProjectDashboardController : BaseController
{
    private readonly IProjectDashboardService _projectDashboardService;
    private readonly IProjectService _projectService;
    private readonly IWorkspaceMemberService _workspaceMemberService;
    private readonly IProjectHealthService _projectHealthService;

    public ProjectDashboardController(
        IProjectDashboardService projectDashboardService,
        IProjectService projectService,
        IWorkspaceMemberService workspaceMemberService,
        IProjectHealthService projectHealthService,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _projectDashboardService = projectDashboardService;
        _projectHealthService = projectHealthService;
        _projectService = projectService;
        _workspaceMemberService = workspaceMemberService;
    }

    public async Task<IActionResult> Index(int projectId)
    {
        var dashboard = await _projectDashboardService.GetDashboardAsync(projectId);
        if (dashboard == null)
            return NotFound();

        var project = await _projectService.GetByIdAsync(projectId);
        if (project == null)
            return NotFound();

        if (!await _workspaceMemberService.IsMemberAsync(project.WorkspaceId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما عضو این Workspace نیستید.";
            return RedirectToAction("Index", "Workspace");
        }

        ViewBag.WorkspaceId = project.WorkspaceId;
        dashboard.Health = await _projectHealthService.GetHealthWithAiAsync(projectId, CurrentUser.UserId);

        return View(dashboard);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GetAiHealthAnalysis(int projectId)
    {
        var project = await _projectService.GetByIdAsync(projectId);
        if (project == null) return Json(new { success = false, message = "پروژه یافت نشد." });

        if (!await _workspaceMemberService.IsMemberAsync(project.WorkspaceId, CurrentUser.UserId))
            return Json(new { success = false, message = "شما عضو این پروژه نیستید." });

        try
        {
            var health = await _projectHealthService.GetHealthWithAiAsync(projectId, CurrentUser.UserId);
            if (health == null)
                return Json(new { success = false, message = "داده‌ای یافت نشد." });

            return Json(new
            {
                success = true,
                healthScore = health.HealthScore,
                healthLevel = health.HealthLevelDisplay,
                aiOverallAssessment = health.AiOverallAssessment,
                aiCriticalAreas = health.AiCriticalAreas,
                aiRecommendations = health.AiRecommendations,
                aiForecast = health.AiForecast
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "خطا: " + ex.Message });
        }
    }
}