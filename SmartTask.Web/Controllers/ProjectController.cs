using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Project;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class ProjectController : BaseController
{
    private readonly IProjectService _projectService;
    private readonly IWorkspaceMemberService _workspaceMemberService;
    private readonly ApplicationDbContext _context;

    public ProjectController(
        IProjectService projectService,
        IWorkspaceMemberService workspaceMemberService,
        ICurrentUserService currentUser,
        ApplicationDbContext context)
        : base(currentUser)
    {
        _projectService = projectService;
        _workspaceMemberService = workspaceMemberService;
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

        var projects = await _context.Projects
            .Where(x => x.WorkspaceId == workspaceId && x.ViewState)
            .Include(x => x.Members)
            .Include(x => x.ProjectTeams)
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => new ProjectListItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Key = x.Key,
                Description = x.Description,
                Color = x.Color ?? "#4F46E5",
                Icon = x.Icon ?? "fa-solid fa-diagram-project",
                Status = x.Status,
                Priority = x.Priority,
                DueDate = x.DueDate,
                IsArchived = x.IsArchived,
                MembersCount = x.Members.Count(m => m.ViewState),
                TeamsCount = x.ProjectTeams.Count(pt => pt.ViewState),
                CreateDate = x.CreatedDate
            })
            .ToListAsync();

        var vm = new ProjectIndexViewModel
        {
            WorkspaceId = workspaceId,
            WorkspaceName = workspace.Name,
            CanManageProjects = await _projectService.CanManageProjectsAsync(workspaceId, CurrentUser.UserId),
            Projects = projects
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var project = await _projectService.GetDetailsAsync(id);

        if (project == null)
            return NotFound();

        if (!await _workspaceMemberService.IsMemberAsync(project.WorkspaceId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما عضو این Workspace نیستید.";
            return RedirectToAction("Index", "Workspace");
        }

        var model = new ProjectDetailsViewModel
        {
            Id = project.Id,
            WorkspaceId = project.WorkspaceId,
            Name = project.Name,
            Key = project.Key,
            Description = project.Description,
            Color = project.Color ?? "#4F46E5",
            Icon = project.Icon ?? "fa-solid fa-diagram-project",
            Status = project.Status,
            Priority = project.Priority,
            StartDate = project.StartDate,
            DueDate = project.DueDate,
            EndDate = project.EndDate,
            IsArchived = project.IsArchived,
            CreateDate = project.CreatedDate,
            CanManage = await _projectService.CanManageProjectAsync(id, CurrentUser.UserId),
            MembersCount = project.Members.Count(m => m.ViewState),
            TeamNames = project.ProjectTeams.Select(pt => pt.Team.Name).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int workspaceId)
    {
        if (!await _projectService.CanManageProjectsAsync(workspaceId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ساخت پروژه در این Workspace را ندارید.";
            return RedirectToAction(nameof(Index), new { workspaceId });
        }

        return View(new CreateProjectViewModel { WorkspaceId = workspaceId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProjectViewModel model)
    {
        if (!await _projectService.CanManageProjectsAsync(model.WorkspaceId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ساخت پروژه در این Workspace را ندارید.";
            return RedirectToAction(nameof(Index), new { workspaceId = model.WorkspaceId });
        }

        if (!ModelState.IsValid)
            return View(model);

        model.Key = model.Key.ToUpper();

        if (await _projectService.ExistsByKeyAsync(model.WorkspaceId, model.Key))
        {
            ModelState.AddModelError("Key", "پروژه‌ای با این کلید قبلاً در این Workspace وجود دارد.");
            return View(model);
        }

        var project = new Project
        {
            WorkspaceId = model.WorkspaceId,
            Name = model.Name,
            Key = model.Key,
            Description = model.Description,
            Color = model.Color,
            Icon = model.Icon,
            StartDate = model.StartDate,
            DueDate = model.DueDate,
            Priority = model.Priority,
            Status = ProjectStatusType.Planning,
            CreatedDate = DateTime.Now,
            ViewState = true
        };

        await _projectService.AddAsync(project);

        // سازنده به‌عنوان Owner پروژه اضافه میشه
        _context.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = project.Id,
            ApplicationUserId = CurrentUser.UserId,
            Role = ProjectRoleType.Owner,
            JoinedDate = DateTime.Now,
            CreatedDate = DateTime.Now,
            ViewState = true
        });
        await _context.SaveChangesAsync();

        TempData["Success"] = "پروژه با موفقیت ایجاد شد.";
        return RedirectToAction(nameof(Details), new { id = project.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == id);

        if (project == null)
            return NotFound();

        if (!await _projectService.CanManageProjectAsync(id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ویرایش این پروژه را ندارید.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var model = new EditProjectViewModel
        {
            Id = project.Id,
            WorkspaceId = project.WorkspaceId,
            Name = project.Name,
            Key = project.Key,
            Description = project.Description,
            Color = project.Color ?? "#4F46E5",
            Icon = project.Icon ?? "fa-solid fa-diagram-project",
            StartDate = project.StartDate,
            DueDate = project.DueDate,
            Status = project.Status,
            Priority = project.Priority,
            IsArchived = project.IsArchived
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProjectViewModel model)
    {
        if (!await _projectService.CanManageProjectAsync(model.Id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ویرایش این پروژه را ندارید.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        if (!ModelState.IsValid)
            return View(model);

        model.Key = model.Key.ToUpper();

        if (await _projectService.ExistsByKeyAsync(model.WorkspaceId, model.Key, model.Id))
        {
            ModelState.AddModelError("Key", "پروژه‌ای با این کلید قبلاً وجود دارد.");
            return View(model);
        }

        var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == model.Id);

        if (project == null)
            return NotFound();

        project.Name = model.Name;
        project.Key = model.Key;
        project.Description = model.Description;
        project.Color = model.Color;
        project.Icon = model.Icon;
        project.StartDate = model.StartDate;
        project.DueDate = model.DueDate;
        project.Status = model.Status;
        project.Priority = model.Priority;
        project.IsArchived = model.IsArchived;
        project.ChangeDate = DateTime.Now;

        if (model.Status == ProjectStatusType.Completed && project.EndDate == null)
            project.EndDate = DateTime.Now;

        await _context.SaveChangesAsync();

        TempData["Success"] = "پروژه با موفقیت ویرایش شد.";
        return RedirectToAction(nameof(Details), new { id = project.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == id);

        if (project == null)
            return NotFound();

        if (!await _projectService.CanManageProjectsAsync(project.WorkspaceId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه حذف این پروژه را ندارید.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var workspaceId = project.WorkspaceId;

        await _projectService.DeleteAsync(id);

        TempData["Success"] = "پروژه با موفقیت حذف شد.";
        return RedirectToAction(nameof(Index), new { workspaceId });
    }
}