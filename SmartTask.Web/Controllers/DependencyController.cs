using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.ViewModels.Dependency;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class DependencyController : BaseController
{
    private readonly ITaskDependencyService _dependencyService;
    private readonly IProjectMemberService _projectMemberService;
    private readonly ApplicationDbContext _context;

    public DependencyController(
        ITaskDependencyService dependencyService,
        IProjectMemberService projectMemberService,
        ICurrentUserService currentUser,
        ApplicationDbContext context)
        : base(currentUser)
    {
        _dependencyService = dependencyService;
        _projectMemberService = projectMemberService;
        _context = context;
    }

    public async Task<IActionResult> Index(int projectId)
    {
        if (!await _projectMemberService.IsMemberAsync(projectId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما عضو این پروژه نیستید.";
            return RedirectToAction("Index", "Project");
        }

        var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == projectId);
        if (project == null)
            return NotFound();

        var risks = await _dependencyService.GetProjectRiskOverviewAsync(projectId);

        var vm = new DependencyRiskIndexViewModel
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            RiskyTasks = risks
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> GraphData(int projectId)
    {
        if (!await _projectMemberService.IsMemberAsync(projectId, CurrentUser.UserId))
            return Forbid();

        var graph = await _dependencyService.GetDependencyGraphAsync(projectId);
        return Json(graph);
    }
}