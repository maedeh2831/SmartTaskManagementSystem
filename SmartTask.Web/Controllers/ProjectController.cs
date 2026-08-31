using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Infrastructure.Services;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Project;
using SmartTask.Web.Services.Interfaces;
using SmartTask.Web.Models.ViewModels.Backlog;
using SmartTask.Web.Models.ViewModels.TaskBoard;
using SmartTask.Web.Models.ViewModels.Sprint;
using SmartTask.Web.Models.ViewModels.ProjectMember;
using SmartTask.Web.Models.ViewModels.Workload;
using SmartTask.Web.Models.ViewModels.Risk;
using SmartTask.Web.Models.ViewModels.Dependency;
using SmartTask.Web.Models.ViewModels.ProjectDashboard;
using SmartTask.Web.Models.ViewModels.Offroad;
using SmartTask.Web.Models.ViewModels.TaskTrade;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartTask.Web.Controllers;

[Authorize]
public class ProjectController : BaseController
{
    private readonly IProjectService _projectService;
    private readonly IWorkspaceMemberService _workspaceMemberService;
    private readonly ApplicationDbContext _context;
    private readonly ICurrentContextService _currentContextService;
    private readonly IProjectDashboardService _projectDashboardService;
    private readonly IProjectHealthService _projectHealthService;
    private readonly IOffroadTaskService _offroadTaskService;
    private readonly ITaskTradeService _taskTradeService;
    private readonly IProjectMemberService _projectMemberService;

    public ProjectController(
        IProjectService projectService,
        IWorkspaceMemberService workspaceMemberService,
        ICurrentUserService currentUser,
        ApplicationDbContext context,
        ICurrentContextService currentContextService,
        IProjectDashboardService projectDashboardService,
        IProjectHealthService projectHealthService,
        IOffroadTaskService offroadTaskService,
        ITaskTradeService taskTradeService,
        IProjectMemberService projectMemberService)
        : base(currentUser)
    {
        _projectService = projectService;
        _workspaceMemberService = workspaceMemberService;
        _context = context;
        _currentContextService = currentContextService;
        _projectDashboardService = projectDashboardService;
        _projectHealthService = projectHealthService;
        _offroadTaskService = offroadTaskService;
        _taskTradeService = taskTradeService;
        _projectMemberService = projectMemberService;
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

        _currentContextService.SetCurrentProject(id);

        var canManage = await _projectService.CanManageProjectAsync(id, CurrentUser.UserId);

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
            CanManage = canManage,
            MembersCount = project.Members.Count(m => m.ViewState),
            TeamNames = project.ProjectTeams.Select(pt => pt.Team.Name).ToList()
        };

        // Pre-load all tab data for single-page rendering
        ViewData["DashboardModel"] = await LoadDashboardTabData(id);
        ViewData["BacklogModel"] = await LoadBacklogTabData(id, canManage);
        ViewData["TaskBoardModel"] = await LoadTaskBoardTabData(id, canManage);
        ViewData["SprintsModel"] = await LoadSprintsTabData(id, canManage);
        ViewData["MembersModel"] = await LoadMembersTabData(id, project);
        ViewData["WorkloadModel"] = await LoadWorkloadTabData(id);
        ViewData["DependencyModel"] = await LoadDependencyTabData(id, project);
        ViewData["DelayRiskModel"] = await LoadDelayRiskTabData(id, project);
        ViewData["OffroadModel"] = await LoadOffroadTabData(id);
        ViewData["TaskTradeModel"] = await LoadTaskTradeTabData(id);

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

    [HttpGet]
    public async Task<IActionResult> Settings(int id)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == id);

        if (project == null)
            return NotFound();

        if (!await _projectService.CanManageProjectAsync(id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه دسترسی به تنظیمات این پروژه را ندارید.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var model = new ProjectSettingsViewModel
        {
            Id = project.Id,
            WorkspaceId = project.WorkspaceId,
            Name = project.Name,
            Key = project.Key,
            Color = project.Color ?? "#4F46E5",
            Icon = project.Icon ?? "fa-solid fa-diagram-project",
            IsArchived = project.IsArchived
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(ProjectSettingsViewModel model)
    {
        if (!await _projectService.CanManageProjectAsync(model.Id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه دسترسی به تنظیمات این پروژه را ندارید.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        if (!ModelState.IsValid)
            return View(model);

        await _projectService.UpdatePreferencesAsync(model.Id, model.Color, model.Icon);

        TempData["Success"] = "تنظیمات پروژه با موفقیت ذخیره شد.";
        return RedirectToAction(nameof(Settings), new { id = model.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archive(int id)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == id);

        if (project == null)
            return NotFound();

        if (!await _projectService.CanManageProjectAsync(id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه بایگانی این پروژه را ندارید.";
            return RedirectToAction(nameof(Settings), new { id });
        }

        if (project.IsArchived)
        {
            TempData["Error"] = "این پروژه در حال حاضر بایگانی شده است.";
            return RedirectToAction(nameof(Settings), new { id });
        }

        await _projectService.ArchiveAsync(id);

        TempData["Success"] = "پروژه با موفقیت بایگانی شد.";
        return RedirectToAction(nameof(Settings), new { id });
    }

    // ==========================================================
    // Tab — Lazy-loaded content for project detail tabs
    // ==========================================================

    [HttpGet]
    public async Task<IActionResult> Tab(int id, string tab)
    {
        var project = await _projectService.GetDetailsAsync(id);
        if (project == null)
            return NotFound();

        if (!await _workspaceMemberService.IsMemberAsync(project.WorkspaceId, CurrentUser.UserId))
            return Forbid();

        var canManage = await _projectService.CanManageProjectAsync(id, CurrentUser.UserId);

        return tab?.ToLower() switch
        {
            "dashboard"  => await RenderDashboardTab(id),
            "backlog"    => await RenderBacklogTab(id, canManage),
            "taskboard"  => await RenderTaskBoardTab(id, canManage),
            "sprints"    => await RenderSprintsTab(id, canManage),
            "members"    => await RenderMembersTab(id, project),
            "workload"   => await RenderWorkloadTab(id),
            "dependency" => await RenderDependencyTab(id, project),
            "delayrisk"  => await RenderDelayRiskTab(id, project),
            "offroad"    => await RenderOffroadTab(id),
            "tasktrade"  => await RenderTaskTradeTab(id),
            _ => Content("<div class='workspace-empty'><p>تب نامعتبر</p></div>")
        };
    }

    // ===== Data loaders for single-page rendering =====

    private async Task<object?> LoadDashboardTabData(int projectId)
    {
        var dashboard = await _projectDashboardService.GetDashboardAsync(projectId);
        if (dashboard == null) return null;
        dashboard.Health = await _projectHealthService.GetHealthWithAiAsync(projectId, CurrentUser.UserId);
        return dashboard;
    }

    private async Task<object?> LoadBacklogTabData(int projectId, bool canManage)
    {
        var service = HttpContext.RequestServices.GetRequiredService<IUserStoryService>();
        var stories = await service.GetBacklogStoriesAsync(projectId);
        var members = await _context.ProjectMembers
            .Where(x => x.ProjectId == projectId && x.ViewState)
            .Include(x => x.ApplicationUser)
            .Select(x => new { x.ApplicationUserId, x.ApplicationUser.FullName })
            .ToListAsync();
        var contributorsMap = await service.GetContributorsMapAsync(projectId);

        var storyIds = stories.Select(s => s.Id).ToList();
        var tasksByStory = await _context.TaskItems
            .Where(t => storyIds.Contains(t.UserStoryId))
            .Include(t => t.Assignments).ThenInclude(a => a.ApplicationUser)
            .GroupBy(t => t.UserStoryId)
            .ToDictionaryAsync(g => g.Key, g => g.ToList());

        return new BacklogIndexViewModel
        {
            ProjectId = projectId,
            ProjectName = (await _context.Projects.FindAsync(projectId))?.Name ?? "",
            CanManage = canManage,
            UnassignedStories = stories.Select(s => new UserStoryListItemViewModel
            {
                Id = s.Id,
                Title = s.Title,
                Status = s.Status,
                Priority = s.Priority,
                StoryPoint = s.StoryPoint,
                BusinessValue = s.BusinessValue,
                OwnerId = s.OwnerId,
                OwnerName = s.Owner?.FullName,
                Contributors = contributorsMap.TryGetValue(s.Id, out var names) ? names : new List<string>(),
                Tasks = tasksByStory.TryGetValue(s.Id, out var tList) ? tList.Select(t => new BacklogTaskItemViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status,
                    Priority = t.Priority,
                    Type = t.Type,
                    Estimate = t.Estimate,
                    DueDate = t.DueDate,
                    AssigneeName = t.Assignments.FirstOrDefault()?.ApplicationUser?.FullName
                }).ToList() : new List<BacklogTaskItemViewModel>()
            }).ToList(),
            ProjectMembers = members.Select(m => new ProjectMemberOptionViewModel
            {
                UserId = m.ApplicationUserId,
                FullName = m.FullName
            }).ToList()
        };
    }

    private async Task<object?> LoadTaskBoardTabData(int projectId, bool canManage)
    {
        var taskService = HttpContext.RequestServices.GetRequiredService<ITaskService>();
        var tasks = await taskService.GetProjectBoardAsync(projectId, null, null, null, null);
        return new TaskBoardViewModel
        {
            ProjectId = projectId,
            ProjectName = (await _context.Projects.FindAsync(projectId))?.Name ?? "",
            CanManage = canManage,
            Tasks = tasks.Select(x => new TaskBoardItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Status = x.Status,
                Priority = x.Priority,
                Type = x.Type,
                Estimate = x.Estimate,
                DueDate = x.DueDate,
                UserStoryId = x.UserStoryId,
                UserStoryTitle = x.UserStory?.Title ?? "",
                AssigneeNames = x.Assignments?.Select(a => a.ApplicationUser?.FullName ?? "").ToList() ?? new List<string>(),
                Labels = x.TaskLabels?.Select(tl => new BoardLabelBadgeViewModel
                {
                    Name = tl.Label?.Name ?? "",
                    Color = tl.Label?.Color ?? ""
                }).ToList() ?? new List<BoardLabelBadgeViewModel>()
            }).ToList()
        };
    }

    private async Task<object?> LoadSprintsTabData(int projectId, bool canManage)
    {
        var service = HttpContext.RequestServices.GetRequiredService<ISprintService>();
        var sprints = await service.GetByProjectAsync(projectId);
        var project = await _context.Projects.FindAsync(projectId);
        return new SmartTask.Web.Models.ViewModels.Sprint.SprintIndexViewModel
        {
            ProjectId = projectId,
            ProjectName = project?.Name ?? "",
            CanManageSprints = canManage,
            Sprints = sprints.Select(x => new SmartTask.Web.Models.ViewModels.Sprint.SprintListItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Goal = x.Goal,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Capacity = x.Capacity,
                Status = x.Status,
                UserStoriesCount = x.UserStories.Count(s => s.ViewState)
            }).ToList()
        };
    }

    private async Task<object?> LoadMembersTabData(int projectId, Project project)
    {
        var members = await _context.ProjectMembers
            .Where(x => x.ProjectId == projectId && x.ViewState)
            .Include(x => x.ApplicationUser)
            .OrderBy(x => x.Role)
            .Select(x => new SmartTask.Web.Models.ViewModels.ProjectMember.ProjectMemberViewModel
            {
                ApplicationUserId = x.ApplicationUserId,
                FullName = x.ApplicationUser.FullName,
                Role = x.Role,
                JoinedDate = x.JoinedDate
            })
            .ToListAsync();

        var memberIds = members.Select(m => m.ApplicationUserId).ToList();
        var available = await _context.WorkspaceMembers
            .Where(wm => wm.WorkspaceId == project.WorkspaceId && wm.ViewState && !memberIds.Contains(wm.ApplicationUserId))
            .Include(wm => wm.ApplicationUser)
            .Select(wm => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = wm.ApplicationUserId.ToString(),
                Text = wm.ApplicationUser.FullName
            })
            .ToListAsync();

        return new SmartTask.Web.Models.ViewModels.ProjectMember.ProjectMemberIndexViewModel
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            ProjectKey = project.Key,
            CanManage = await _projectService.CanManageProjectAsync(projectId, CurrentUser.UserId),
            Members = members,
            AvailableWorkspaceMembers = available
        };
    }

    private async Task<object?> LoadWorkloadTabData(int projectId)
    {
        var service = HttpContext.RequestServices.GetRequiredService<IWorkloadAnalysisService>();
        return await service.GetWorkloadAsync(projectId, CurrentUser.UserId);
    }

    private async Task<object?> LoadDependencyTabData(int projectId, Project project)
    {
        var service = HttpContext.RequestServices.GetRequiredService<ITaskDependencyService>();
        var risks = await service.GetProjectRiskOverviewAsync(projectId);
        return new DependencyRiskIndexViewModel
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            RiskyTasks = risks
        };
    }

    private async Task<object?> LoadDelayRiskTabData(int projectId, Project project)
    {
        var service = HttpContext.RequestServices.GetRequiredService<IDelayRiskService>();
        return await service.GetRiskOverviewAsync(projectId, CurrentUser.UserId);
    }

    private async Task<OffroadIndexViewModel?> LoadOffroadTabData(int projectId)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == projectId);
        if (project == null) return null;

        var tasks = await _offroadTaskService.GetByProjectAsync(projectId);
        var currentUserId = CurrentUser.UserId;

        var members = await _context.ProjectMembers
            .Where(x => x.ProjectId == projectId && x.ViewState)
            .Include(x => x.ApplicationUser)
            .ToListAsync();

        return new OffroadIndexViewModel
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            ProjectMembers = members
                .Select(x => new SelectListItem
                {
                    Value = x.ApplicationUserId.ToString(),
                    Text = x.ApplicationUser.FullName
                }).ToList(),
            Tasks = tasks.Select(x => new OffroadTaskListItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Status = x.Status,
                Priority = x.Priority,
                CreatedByName = x.CreatedByUser.FullName,
                AssignedToName = x.AssignedToUser?.FullName,
                AssignedToUserId = x.AssignedToUserId,
                DueDate = x.DueDate,
                CreateDate = x.CreatedDate,
                CanManage = x.CreatedByUserId == currentUserId
            }).ToList()
        };
    }

    private async Task<TaskTradeIndexViewModel?> LoadTaskTradeTabData(int projectId)
    {
        return await _taskTradeService.GetProjectRequestsAsync(projectId, CurrentUser.UserId);
    }

    // ===== Renderers for AJAX Tab endpoint (kept for backward compat) =====

    private async Task<IActionResult> RenderDashboardTab(int projectId)
    {
        var dashboard = await _projectDashboardService.GetDashboardAsync(projectId);
        if (dashboard == null) return Content("<div class='workspace-empty'><p>داشبورد یافت نشد</p></div>");
        dashboard.Health = await _projectHealthService.GetHealthWithAiAsync(projectId, CurrentUser.UserId);
        return PartialView("/Views/ProjectDashboard/_DashboardPartial.cshtml", dashboard);
    }

    private async Task<IActionResult> RenderBacklogTab(int projectId, bool canManage)
    {
        var service = HttpContext.RequestServices.GetRequiredService<IUserStoryService>();
        var stories = await service.GetBacklogStoriesAsync(projectId);
        var members = await _context.ProjectMembers
            .Where(x => x.ProjectId == projectId && x.ViewState)
            .Include(x => x.ApplicationUser)
            .Select(x => new { x.ApplicationUserId, x.ApplicationUser.FullName })
            .ToListAsync();
        var contributorsMap = await service.GetContributorsMapAsync(projectId);

        var storyIds = stories.Select(s => s.Id).ToList();
        var tasksByStory = await _context.TaskItems
            .Where(t => storyIds.Contains(t.UserStoryId))
            .Include(t => t.Assignments).ThenInclude(a => a.ApplicationUser)
            .GroupBy(t => t.UserStoryId)
            .ToDictionaryAsync(g => g.Key, g => g.ToList());

        var vm = new BacklogIndexViewModel
        {
            ProjectId = projectId,
            ProjectName = (await _context.Projects.FindAsync(projectId))?.Name ?? "",
            CanManage = canManage,
            UnassignedStories = stories.Select(s => new UserStoryListItemViewModel
            {
                Id = s.Id,
                Title = s.Title,
                Status = s.Status,
                Priority = s.Priority,
                StoryPoint = s.StoryPoint,
                BusinessValue = s.BusinessValue,
                OwnerId = s.OwnerId,
                OwnerName = s.Owner?.FullName,
                Contributors = contributorsMap.TryGetValue(s.Id, out var names) ? names : new List<string>(),
                Tasks = tasksByStory.TryGetValue(s.Id, out var tList) ? tList.Select(t => new BacklogTaskItemViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    Status = t.Status,
                    Priority = t.Priority,
                    Type = t.Type,
                    Estimate = t.Estimate,
                    DueDate = t.DueDate,
                    AssigneeName = t.Assignments.FirstOrDefault()?.ApplicationUser?.FullName
                }).ToList() : new List<BacklogTaskItemViewModel>()
            }).ToList(),
            ProjectMembers = members.Select(m => new ProjectMemberOptionViewModel
            {
                UserId = m.ApplicationUserId,
                FullName = m.FullName
            }).ToList()
        };
        return PartialView("/Views/Backlog/_BacklogPartial.cshtml", vm);
    }

    private async Task<IActionResult> RenderTaskBoardTab(int projectId, bool canManage)
    {
        return await GetTaskBoardPartial(projectId, canManage, null, null, null, null);
    }

    [HttpGet]
    public async Task<IActionResult> TaskBoardFiltered(int projectId, int? assigneeId, TaskPriorityType? priority, TaskType? type, int? labelId)
    {
        if (!await _projectMemberService.IsMemberAsync(projectId, CurrentUser.UserId))
            return Content("");
        var canManage = await _projectService.CanManageProjectsAsync(projectId, CurrentUser.UserId);
        return await GetTaskBoardPartial(projectId, canManage, assigneeId, priority, type, labelId);
    }

    private async Task<IActionResult> GetTaskBoardPartial(int projectId, bool canManage, int? assigneeId, TaskPriorityType? priority, TaskType? type, int? labelId)
    {
        var taskService = HttpContext.RequestServices.GetRequiredService<ITaskService>();
        var labelService = HttpContext.RequestServices.GetRequiredService<ILabelService>();
        var tasks = await taskService.GetProjectBoardAsync(projectId, assigneeId, priority, type, labelId);
        var projectMembers = await _context.ProjectMembers
            .Where(x => x.ProjectId == projectId && x.ViewState)
            .Include(x => x.ApplicationUser)
            .ToListAsync();
        var labels = await labelService.GetByProjectAsync(projectId);
        var vm = new TaskBoardViewModel
        {
            ProjectId = projectId,
            ProjectName = (await _context.Projects.FindAsync(projectId))?.Name ?? "",
            CanManage = canManage,
            SelectedAssigneeId = assigneeId,
            SelectedPriority = priority,
            SelectedType = type,
            SelectedLabelId = labelId,
            AvailableAssignees = projectMembers.Select(x => new BoardFilterOptionViewModel
            {
                Id = x.ApplicationUserId,
                Name = x.ApplicationUser.FullName
            }).ToList(),
            AvailableLabels = labels.Select(x => new BoardFilterOptionViewModel
            {
                Id = x.Id,
                Name = x.Name
            }).ToList(),
            Tasks = tasks.Select(x => new TaskBoardItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Status = x.Status,
                Priority = x.Priority,
                Type = x.Type,
                Estimate = x.Estimate,
                DueDate = x.DueDate,
                UserStoryId = x.UserStoryId,
                UserStoryTitle = x.UserStory?.Title ?? "",
                AssigneeNames = x.Assignments?.Select(a => a.ApplicationUser?.FullName ?? "").ToList() ?? new List<string>(),
                Labels = x.TaskLabels?.Select(tl => new BoardLabelBadgeViewModel
                {
                    Name = tl.Label?.Name ?? "",
                    Color = tl.Label?.Color ?? ""
                }).ToList() ?? new List<BoardLabelBadgeViewModel>()
            }).ToList()
        };
        return PartialView("/Views/TaskBoard/_BoardPartial.cshtml", vm);
    }

    private async Task<IActionResult> RenderSprintsTab(int projectId, bool canManage)
    {
        var service = HttpContext.RequestServices.GetRequiredService<ISprintService>();
        var sprints = await service.GetByProjectAsync(projectId);
        var project = await _context.Projects.FindAsync(projectId);
        var vm = new SmartTask.Web.Models.ViewModels.Sprint.SprintIndexViewModel
        {
            ProjectId = projectId,
            ProjectName = project?.Name ?? "",
            CanManageSprints = canManage,
            Sprints = sprints.Select(x => new SmartTask.Web.Models.ViewModels.Sprint.SprintListItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Goal = x.Goal,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                Capacity = x.Capacity,
                Status = x.Status,
                UserStoriesCount = x.UserStories.Count(s => s.ViewState)
            }).ToList()
        };
        return PartialView("/Views/Sprint/_SprintListPartial.cshtml", vm);
    }

    private async Task<IActionResult> RenderMembersTab(int projectId, Project project)
    {
        var members = await _context.ProjectMembers
            .Where(x => x.ProjectId == projectId && x.ViewState)
            .Include(x => x.ApplicationUser)
            .OrderBy(x => x.Role)
            .Select(x => new SmartTask.Web.Models.ViewModels.ProjectMember.ProjectMemberViewModel
            {
                ApplicationUserId = x.ApplicationUserId,
                FullName = x.ApplicationUser.FullName,
                Role = x.Role,
                JoinedDate = x.JoinedDate
            })
            .ToListAsync();

        var memberIds = members.Select(m => m.ApplicationUserId).ToList();
        var available = await _context.WorkspaceMembers
            .Where(wm => wm.WorkspaceId == project.WorkspaceId && wm.ViewState && !memberIds.Contains(wm.ApplicationUserId))
            .Include(wm => wm.ApplicationUser)
            .Select(wm => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = wm.ApplicationUserId.ToString(),
                Text = wm.ApplicationUser.FullName
            })
            .ToListAsync();

        var vm = new SmartTask.Web.Models.ViewModels.ProjectMember.ProjectMemberIndexViewModel
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            ProjectKey = project.Key,
            CanManage = await _projectService.CanManageProjectAsync(projectId, CurrentUser.UserId),
            Members = members,
            AvailableWorkspaceMembers = available
        };
        return PartialView("/Views/ProjectMember/_MembersPartial.cshtml", vm);
    }

    private async Task<IActionResult> RenderWorkloadTab(int projectId)
    {
        var service = HttpContext.RequestServices.GetRequiredService<IWorkloadAnalysisService>();
        var vm = await service.GetWorkloadAsync(projectId, CurrentUser.UserId);
        if (vm == null) return Content("<div class='workspace-empty'><p>داده‌ای یافت نشد</p></div>");
        return PartialView("/Views/Workload/_WorkloadPartial.cshtml", vm);
    }

    private async Task<IActionResult> RenderDependencyTab(int projectId, Project project)
    {
        var service = HttpContext.RequestServices.GetRequiredService<ITaskDependencyService>();
        var risks = await service.GetProjectRiskOverviewAsync(projectId);
        var vm = new DependencyRiskIndexViewModel
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            RiskyTasks = risks
        };
        return PartialView("/Views/Dependency/_DependencyPartial.cshtml", vm);
    }

    private async Task<IActionResult> RenderDelayRiskTab(int projectId, Project project)
    {
        var service = HttpContext.RequestServices.GetRequiredService<IDelayRiskService>();
        var vm = await service.GetRiskOverviewAsync(projectId, CurrentUser.UserId);
        if (vm == null) return Content("<div class='workspace-empty'><p>داده‌ای یافت نشد</p></div>");
        return PartialView("/Views/DelayRisk/_DelayRiskPartial.cshtml", vm);
    }

    private Task<IActionResult> RenderOffroadTab(int projectId)
    {
        ViewData["ProjectId"] = projectId;
        return Task.FromResult<IActionResult>(PartialView("/Views/Offroad/_OffroadPartial.cshtml"));
    }

    private Task<IActionResult> RenderTaskTradeTab(int projectId)
    {
        ViewData["ProjectId"] = projectId;
        return Task.FromResult<IActionResult>(PartialView("/Views/TaskTrade/_TaskTradePartial.cshtml"));
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == id);

        if (project == null)
            return NotFound();

        if (!await _projectService.CanManageProjectAsync(id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه بازگردانی این پروژه را ندارید.";
            return RedirectToAction(nameof(Settings), new { id });
        }

        if (!project.IsArchived)
        {
            TempData["Error"] = "این پروژه بایگانی نشده است.";
            return RedirectToAction(nameof(Settings), new { id });
        }

        await _projectService.RestoreAsync(id);

        TempData["Success"] = "پروژه با موفقیت بازگردانی شد.";
        return RedirectToAction(nameof(Settings), new { id });
    }
}