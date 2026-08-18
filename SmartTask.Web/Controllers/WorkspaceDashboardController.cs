using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class WorkspaceDashboardController : BaseController
{
    private readonly IWorkspaceDashboardService _dashboardService;
    private readonly IWorkspaceMemberService _workspaceMemberService;
    private readonly ApplicationDbContext _context;

    public WorkspaceDashboardController(
        IWorkspaceDashboardService dashboardService,
        IWorkspaceMemberService workspaceMemberService,
        ICurrentUserService currentUser,
        ApplicationDbContext context)
        : base(currentUser)
    {
        _dashboardService = dashboardService;
        _workspaceMemberService = workspaceMemberService;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int workspaceId)
    {
        // Validate workspace exists before anything else
        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(x => x.Id == workspaceId && x.ViewState);

        if (workspace == null)
        {
            // Silent redirect — no error popup on login
            return RedirectToAction("Index", "Workspace");
        }

        if (!await _workspaceMemberService.IsMemberAsync(workspaceId, CurrentUser.UserId))
        {
            return RedirectToAction("Index", "Workspace");
        }

        var model = await _dashboardService.GetDashboardAsync(workspaceId, CurrentUser.UserId);
        return View(model);
    }
}