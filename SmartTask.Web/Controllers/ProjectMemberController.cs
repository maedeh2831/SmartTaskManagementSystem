using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.ProjectMember;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class ProjectMemberController : BaseController
{
    private readonly IProjectMemberService _projectMemberService;
    private readonly IProjectService _projectService;
    private readonly IWorkspaceMemberService _workspaceMemberService;
    private readonly ApplicationDbContext _context;

    public ProjectMemberController(
        IProjectMemberService projectMemberService,
        IProjectService projectService,
        IWorkspaceMemberService workspaceMemberService,
        ICurrentUserService currentUser,
        ApplicationDbContext context)
        : base(currentUser)
    {
        _projectMemberService = projectMemberService;
        _projectService = projectService;
        _workspaceMemberService = workspaceMemberService;
        _context = context;
    }

    public async Task<IActionResult> Index(int projectId)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == projectId && x.ViewState);

        if (project == null)
            return NotFound();

        if (!await _workspaceMemberService.IsMemberAsync(project.WorkspaceId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما عضو این Workspace نیستید.";
            return RedirectToAction("Index", "Workspace");
        }

        var members = await _context.ProjectMembers
            .Where(x => x.ProjectId == projectId && x.ViewState)
            .Include(x => x.ApplicationUser)
            .OrderBy(x => x.Role)
            .Select(x => new ProjectMemberViewModel
            {
                ApplicationUserId = x.ApplicationUserId,
                FullName = x.ApplicationUser.FullName,
                Role = x.Role,
                JoinedDate = x.JoinedDate
            })
            .ToListAsync();

        var memberUserIds = members.Select(m => m.ApplicationUserId).ToList();

        var availableMembers = await _context.WorkspaceMembers
            .Where(wm =>
                wm.WorkspaceId == project.WorkspaceId &&
                wm.ViewState &&
                !memberUserIds.Contains(wm.ApplicationUserId))
            .Include(wm => wm.ApplicationUser)
            .Select(wm => new SelectListItem
            {
                Value = wm.ApplicationUserId.ToString(),
                Text = wm.ApplicationUser.FullName
            })
            .ToListAsync();

        var vm = new ProjectMemberIndexViewModel
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            ProjectKey = project.Key,
            CanManage = await _projectService.CanManageProjectAsync(projectId, CurrentUser.UserId),
            Members = members,
            AvailableWorkspaceMembers = availableMembers
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMember(int projectId, int userId, ProjectRoleType role)
    {
        if (!await _projectService.CanManageProjectAsync(projectId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه مدیریت اعضای این پروژه را ندارید.";
            return RedirectToAction(nameof(Index), new { projectId });
        }

        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == projectId && x.ViewState);

        if (project == null)
            return NotFound();

        var isWorkspaceMember = await _context.WorkspaceMembers
            .AnyAsync(x => x.WorkspaceId == project.WorkspaceId && x.ApplicationUserId == userId && x.ViewState);

        if (!isWorkspaceMember)
        {
            TempData["Error"] = "این کاربر عضو Workspace نیست و نمی‌تواند به پروژه اضافه شود.";
            return RedirectToAction(nameof(Index), new { projectId });
        }

        await _projectMemberService.AddMemberAsync(projectId, userId, role);

        TempData["Success"] = "عضو با موفقیت به پروژه اضافه شد.";
        return RedirectToAction(nameof(Index), new { projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(int projectId, int userId)
    {
        if (!await _projectService.CanManageProjectAsync(projectId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه مدیریت اعضای این پروژه را ندارید.";
            return RedirectToAction(nameof(Index), new { projectId });
        }

        try
        {
            await _projectMemberService.RemoveMemberAsync(projectId, userId);
            TempData["Success"] = "عضو از پروژه حذف شد.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(int projectId, int userId, ProjectRoleType role)
    {
        if (!await _projectService.CanManageProjectAsync(projectId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه مدیریت اعضای این پروژه را ندارید.";
            return RedirectToAction(nameof(Index), new { projectId });
        }

        try
        {
            await _projectMemberService.ChangeRoleAsync(projectId, userId, role);
            TempData["Success"] = "نقش عضو با موفقیت تغییر کرد.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { projectId });
    }
}