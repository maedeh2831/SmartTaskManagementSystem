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

    public ProjectDashboardController(
        IProjectDashboardService projectDashboardService,
        IProjectService projectService,
        IWorkspaceMemberService workspaceMemberService,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _projectDashboardService = projectDashboardService;
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

        return View(dashboard);
    }
}