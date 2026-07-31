using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Team;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class TeamController : BaseController
{
    private readonly ITeamService _teamService;
    private readonly ITeamMemberService _teamMemberService;
    private readonly IWorkspaceMemberService _workspaceMemberService;
    private readonly IProjectTeamService _projectTeamService;
    private readonly ApplicationDbContext _context;

    public TeamController(
        ITeamService teamService,
        ITeamMemberService teamMemberService,
        IWorkspaceMemberService workspaceMemberService,
        IProjectTeamService projectTeamService,
        ICurrentUserService currentUser,
        ApplicationDbContext context)
        : base(currentUser)
    {
        _teamService = teamService;
        _teamMemberService = teamMemberService;
        _workspaceMemberService = workspaceMemberService;
        _projectTeamService = projectTeamService;
        _context = context;
    }

    public async Task<IActionResult> Index(int workspaceId)
    {
        if (!await _workspaceMemberService.IsMemberAsync(workspaceId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما عضو این Workspace نیستید.";
            return RedirectToAction("Index", "Workspace");
        }

        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(x => x.Id == workspaceId);

        if (workspace == null)
            return NotFound();

        var teams = await _context.Teams
            .Where(x => x.WorkspaceId == workspaceId && x.ViewState)
            .Include(x => x.Members)
            .Include(x => x.ProjectTeams)
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => new TeamListItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Color = x.Color ?? "#4F46E5",
                Logo = x.Logo,
                IsPrivate = x.IsPrivate,
                IsArchived = x.IsArchived,
                MembersCount = x.Members.Count(m => m.ViewState),
                ProjectsCount = x.ProjectTeams.Count(pt => pt.ViewState),
                CreateDate = x.CreatedDate
            })
            .ToListAsync();

        var vm = new TeamIndexViewModel
        {
            WorkspaceId = workspaceId,
            WorkspaceName = workspace.Name,
            CanManageTeams = await _teamService.CanManageTeamsAsync(workspaceId, CurrentUser.UserId),
            Teams = teams
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var team = await _teamService.GetDetailsAsync(id);

        if (team == null)
            return NotFound();

        if (!await _workspaceMemberService.IsMemberAsync(team.WorkspaceId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما عضو این Workspace نیستید.";
            return RedirectToAction("Index", "Workspace");
        }

        var memberUserIds = team.Members.Select(m => m.ApplicationUserId).ToList();

        var availableMembers = await _context.WorkspaceMembers
            .Where(wm =>
                wm.WorkspaceId == team.WorkspaceId &&
                wm.ViewState &&
                !memberUserIds.Contains(wm.ApplicationUserId))
            .Include(wm => wm.ApplicationUser)
            .Select(wm => new SelectListItem
            {
                Value = wm.ApplicationUserId.ToString(),
                Text = wm.ApplicationUser.FullName
            })
            .ToListAsync();

        var projects = await _projectTeamService.GetProjectsForTeamAsync(id);
        var availableProjects = await _projectTeamService.GetAvailableProjectsAsync(team.WorkspaceId, id);

        var model = new TeamDetailsViewModel
        {
            Id = team.Id,
            WorkspaceId = team.WorkspaceId,
            Name = team.Name,
            Description = team.Description,
            Color = team.Color ?? "#4F46E5",
            Logo = team.Logo,
            IsPrivate = team.IsPrivate,
            IsArchived = team.IsArchived,
            CreateDate = team.CreatedDate,
            CanManage = await _teamService.CanManageTeamAsync(id, CurrentUser.UserId),
            Members = team.Members
                .OrderBy(m => m.Role)
                .Select(m => new TeamMemberViewModel
                {
                    ApplicationUserId = m.ApplicationUserId,
                    FullName = m.ApplicationUser.FullName,
                    Role = m.Role,
                    JoinedDate = m.JoinedDate
                })
                .ToList(),
            Projects = projects.Select(p => new ProjectTeamItemViewModel
            {
                ProjectId = p.ProjectId,
                ProjectName = p.ProjectName
            }).ToList(),
            AvailableWorkspaceMembers = availableMembers,
            AvailableProjects = availableProjects
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int workspaceId)
    {
        if (!await _teamService.CanManageTeamsAsync(workspaceId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ساخت تیم در این Workspace را ندارید.";
            return RedirectToAction(nameof(Index), new { workspaceId });
        }

        return View(new CreateTeamViewModel { WorkspaceId = workspaceId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTeamViewModel model)
    {
        if (!await _teamService.CanManageTeamsAsync(model.WorkspaceId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ساخت تیم در این Workspace را ندارید.";
            return RedirectToAction(nameof(Index), new { workspaceId = model.WorkspaceId });
        }

        if (!ModelState.IsValid)
            return View(model);

        if (await _teamService.ExistsByNameAsync(model.WorkspaceId, model.Name))
        {
            ModelState.AddModelError("Name", "تیمی با این نام قبلاً وجود دارد.");
            return View(model);
        }

        var team = new Team
        {
            WorkspaceId = model.WorkspaceId,
            Name = model.Name,
            Description = model.Description,
            Color = model.Color,
            IsPrivate = model.IsPrivate,
            CreatedDate = DateTime.Now,
            ViewState = true
        };

        await _teamService.AddAsync(team);

        // سازنده به‌عنوان Leader تیم اضافه میشه
        await _teamMemberService.AddMemberAsync(team.Id, CurrentUser.UserId, TeamRoleType.Leader);

        TempData["Success"] = "تیم با موفقیت ایجاد شد.";
        return RedirectToAction(nameof(Details), new { id = team.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var team = await _context.Teams.FirstOrDefaultAsync(x => x.Id == id);

        if (team == null)
            return NotFound();

        if (!await _teamService.CanManageTeamAsync(id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ویرایش این تیم را ندارید.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var model = new EditTeamViewModel
        {
            Id = team.Id,
            WorkspaceId = team.WorkspaceId,
            Name = team.Name,
            Description = team.Description,
            Color = team.Color ?? "#4F46E5",
            IsPrivate = team.IsPrivate,
            IsArchived = team.IsArchived
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditTeamViewModel model)
    {
        if (!await _teamService.CanManageTeamAsync(model.Id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ویرایش این تیم را ندارید.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        if (!ModelState.IsValid)
            return View(model);

        if (await _teamService.ExistsByNameAsync(model.WorkspaceId, model.Name, model.Id))
        {
            ModelState.AddModelError("Name", "تیمی با این نام قبلاً وجود دارد.");
            return View(model);
        }

        var team = await _context.Teams.FirstOrDefaultAsync(x => x.Id == model.Id);

        if (team == null)
            return NotFound();

        team.Name = model.Name;
        team.Description = model.Description;
        team.Color = model.Color;
        team.IsPrivate = model.IsPrivate;
        team.IsArchived = model.IsArchived;
        team.ChangeDate = DateTime.Now;

        await _context.SaveChangesAsync();

        TempData["Success"] = "تیم با موفقیت ویرایش شد.";
        return RedirectToAction(nameof(Details), new { id = team.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var team = await _context.Teams.FirstOrDefaultAsync(x => x.Id == id);

        if (team == null)
            return NotFound();

        if (!await _teamService.CanManageTeamsAsync(team.WorkspaceId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه حذف این تیم را ندارید.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var workspaceId = team.WorkspaceId;

        await _teamService.DeleteAsync(id);

        TempData["Success"] = "تیم با موفقیت حذف شد.";
        return RedirectToAction(nameof(Index), new { workspaceId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMember(int teamId, int userId, TeamRoleType role)
    {
        if (!await _teamService.CanManageTeamAsync(teamId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه مدیریت اعضای این تیم را ندارید.";
            return RedirectToAction(nameof(Details), new { id = teamId });
        }

        await _teamMemberService.AddMemberAsync(teamId, userId, role);

        TempData["Success"] = "عضو با موفقیت اضافه شد.";
        return RedirectToAction(nameof(Details), new { id = teamId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(int teamId, int userId)
    {
        if (!await _teamService.CanManageTeamAsync(teamId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه مدیریت اعضای این تیم را ندارید.";
            return RedirectToAction(nameof(Details), new { id = teamId });
        }

        await _teamMemberService.RemoveMemberAsync(teamId, userId);

        TempData["Success"] = "عضو از تیم حذف شد.";
        return RedirectToAction(nameof(Details), new { id = teamId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(int teamId, int userId, TeamRoleType role)
    {
        if (!await _teamService.CanManageTeamAsync(teamId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه مدیریت اعضای این تیم را ندارید.";
            return RedirectToAction(nameof(Details), new { id = teamId });
        }

        await _teamMemberService.ChangeRoleAsync(teamId, userId, role);

        TempData["Success"] = "نقش عضو با موفقیت تغییر کرد.";
        return RedirectToAction(nameof(Details), new { id = teamId });
    }
}