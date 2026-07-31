using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class ProjectTeamController : BaseController
{
    private readonly IProjectTeamService _projectTeamService;
    private readonly ITeamService _teamService;

    public ProjectTeamController(
        IProjectTeamService projectTeamService,
        ITeamService teamService,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _projectTeamService = projectTeamService;
        _teamService = teamService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignProject(int teamId, int projectId)
    {
        if (!await _teamService.CanManageTeamAsync(teamId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه مدیریت پروژه‌های این تیم را ندارید.";
            return RedirectToAction("Details", "Team", new { id = teamId });
        }

        await _projectTeamService.AssignTeamToProjectAsync(projectId, teamId);

        TempData["Success"] = "پروژه با موفقیت به تیم اضافه شد.";
        return RedirectToAction("Details", "Team", new { id = teamId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveProject(int teamId, int projectId)
    {
        if (!await _teamService.CanManageTeamAsync(teamId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه مدیریت پروژه‌های این تیم را ندارید.";
            return RedirectToAction("Details", "Team", new { id = teamId });
        }

        await _projectTeamService.RemoveTeamFromProjectAsync(projectId, teamId);

        TempData["Success"] = "پروژه از تیم حذف شد.";
        return RedirectToAction("Details", "Team", new { id = teamId });
    }
}