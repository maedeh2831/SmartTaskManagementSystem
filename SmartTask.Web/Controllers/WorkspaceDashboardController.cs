using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class WorkspaceDashboardController : BaseController
{
    private readonly IWorkspaceDashboardService _dashboardService;
    private readonly IWorkspaceMemberService _workspaceMemberService;

    public WorkspaceDashboardController(
        IWorkspaceDashboardService dashboardService,
        IWorkspaceMemberService workspaceMemberService,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _dashboardService = dashboardService;
        _workspaceMemberService = workspaceMemberService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int workspaceId)
    {
        if (!await _workspaceMemberService.IsMemberAsync(workspaceId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما عضو این Workspace نیستید.";
            return RedirectToAction("Index", "Workspace");
        }

        try
        {
            var model = await _dashboardService.GetDashboardAsync(workspaceId, CurrentUser.UserId);
            return View(model);
        }
        catch (Exception)
        {
            TempData["Error"] = "فضای کاری یافت نشد.";
            return RedirectToAction("Index", "Workspace");
        }
    }
}