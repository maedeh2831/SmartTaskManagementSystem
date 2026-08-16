using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Sprint;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class SprintController : BaseController
{
    private readonly ISprintService _sprintService;
    private readonly IUserStoryService _userStoryService;
    private readonly ApplicationDbContext _context;

    public SprintController(
        ISprintService sprintService,
        IUserStoryService userStoryService,
        ICurrentUserService currentUser,
        ApplicationDbContext context)
        : base(currentUser)
    {
        _sprintService = sprintService;
        _userStoryService = userStoryService;
        _context = context;
    }

    public async Task<IActionResult> Index(int projectId)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == projectId);

        if (project == null)
            return NotFound();

        var sprints = await _sprintService.GetByProjectAsync(projectId);

        var vm = new SprintIndexViewModel
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            CanManageSprints = await _sprintService.CanManageSprintsAsync(projectId, CurrentUser.UserId),
            Sprints = sprints.Select(x => new SprintListItemViewModel
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

        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var sprint = await _sprintService.GetDetailsAsync(id);

        if (sprint == null)
            return NotFound();

        var stories = await _context.UserStories
            .Where(x => x.SprintId == id && x.ViewState)
            .Include(x => x.Owner)
            .OrderBy(x => x.Order)
            .ToListAsync();

        var model = new SprintDetailsViewModel
        {
            Id = sprint.Id,
            ProjectId = sprint.ProjectId,
            ProjectName = sprint.Project.Name,
            Name = sprint.Name,
            Goal = sprint.Goal,
            StartDate = sprint.StartDate,
            EndDate = sprint.EndDate,
            Capacity = sprint.Capacity,
            Status = sprint.Status,
            CreateDate = sprint.CreatedDate,
            CanManage = await _sprintService.CanManageSprintAsync(id, CurrentUser.UserId),
            Stories = stories.Select(x => new PlanningStoryItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                StoryPoint = x.StoryPoint,
                Priority = x.Priority,
                Status = x.Status,
                OwnerName = x.Owner != null ? x.Owner.FullName : null
            }).ToList()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int projectId)
    {
        if (!await _sprintService.CanManageSprintsAsync(projectId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ساخت اسپرینت در این پروژه را ندارید.";
            return RedirectToAction(nameof(Index), new { projectId });
        }

        return View(new CreateSprintViewModel { ProjectId = projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSprintViewModel model)
    {
        if (!await _sprintService.CanManageSprintsAsync(model.ProjectId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ساخت اسپرینت در این پروژه را ندارید.";
            return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
        }

        if (!ModelState.IsValid)
            return View(model);

        if (await _sprintService.ExistsByNameAsync(model.ProjectId, model.Name))
        {
            ModelState.AddModelError("Name", "اسپرینتی با این نام قبلاً وجود دارد.");
            return View(model);
        }

        if (await _sprintService.HasDateOverlapAsync(model.ProjectId, model.StartDate, model.EndDate))
        {
            ModelState.AddModelError("StartDate", "این بازه زمانی با یکی از اسپرینت‌های دیگر این پروژه تداخل دارد.");
            return View(model);
        }

        var sprint = new Sprint
        {
            ProjectId = model.ProjectId,
            Name = model.Name,
            Goal = model.Goal,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Capacity = model.Capacity,
            Status = SprintStatusType.Planning,
            CreatedDate = DateTime.Now,
            ViewState = true
        };

        await _sprintService.AddAsync(sprint);

        TempData["Success"] = "اسپرینت با موفقیت ایجاد شد.";
        return RedirectToAction(nameof(Details), new { id = sprint.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var sprint = await _context.Sprints.FirstOrDefaultAsync(x => x.Id == id);

        if (sprint == null)
            return NotFound();

        if (!await _sprintService.CanManageSprintAsync(id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ویرایش این اسپرینت را ندارید.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var model = new EditSprintViewModel
        {
            Id = sprint.Id,
            ProjectId = sprint.ProjectId,
            Name = sprint.Name,
            Goal = sprint.Goal,
            StartDate = sprint.StartDate,
            EndDate = sprint.EndDate,
            Capacity = sprint.Capacity
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditSprintViewModel model)
    {
        if (!await _sprintService.CanManageSprintAsync(model.Id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ویرایش این اسپرینت را ندارید.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        if (!ModelState.IsValid)
            return View(model);

        if (await _sprintService.ExistsByNameAsync(model.ProjectId, model.Name, model.Id))
        {
            ModelState.AddModelError("Name", "اسپرینتی با این نام قبلاً وجود دارد.");
            return View(model);
        }

        if (await _sprintService.HasDateOverlapAsync(model.ProjectId, model.StartDate, model.EndDate, model.Id))
        {
            ModelState.AddModelError("StartDate", "این بازه زمانی با یکی از اسپرینت‌های دیگر این پروژه تداخل دارد.");
            return View(model);
        }

        var sprint = await _context.Sprints.FirstOrDefaultAsync(x => x.Id == model.Id);

        if (sprint == null)
            return NotFound();

        sprint.Name = model.Name;
        sprint.Goal = model.Goal;
        sprint.StartDate = model.StartDate;
        sprint.EndDate = model.EndDate;
        sprint.Capacity = model.Capacity;
        sprint.ChangeDate = DateTime.Now;

        await _context.SaveChangesAsync();

        TempData["Success"] = "اسپرینت با موفقیت ویرایش شد.";
        return RedirectToAction(nameof(Details), new { id = sprint.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var sprint = await _context.Sprints.FirstOrDefaultAsync(x => x.Id == id);

        if (sprint == null)
            return NotFound();

        if (!await _sprintService.CanManageSprintAsync(id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه حذف این اسپرینت را ندارید.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var projectId = sprint.ProjectId;

        await _sprintService.DeleteAsync(id);

        TempData["Success"] = "اسپرینت با موفقیت حذف شد.";
        return RedirectToAction(nameof(Index), new { projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        if (!await _sprintService.CanManageSprintAsync(id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه فعال‌سازی این اسپرینت را ندارید.";
            return RedirectToAction(nameof(Details), new { id });
        }

        await _sprintService.ActivateAsync(id);

        TempData["Success"] = "اسپرینت فعال شد.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        if (!await _sprintService.CanManageSprintAsync(id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه بستن این اسپرینت را ندارید.";
            return RedirectToAction(nameof(Details), new { id });
        }

        await _sprintService.CompleteAsync(id);

        TempData["Success"] = "اسپرینت با موفقیت بسته شد.";
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Planning(int id)
    {
        var sprint = await _context.Sprints.FirstOrDefaultAsync(x => x.Id == id && x.ViewState);
        if (sprint == null)
            return NotFound();
        return RedirectToAction(nameof(Details), new { id, tab = "planning" });
    }

    [HttpGet]
    public async Task<IActionResult> PlanningTab(int id)
    {
        var vm = await BuildPlanningViewModelAsync(id);

        if (vm == null)
            return NotFound();

        return PartialView("_PlanningPartial", vm);
    }

    private async Task<SprintPlanningViewModel?> BuildPlanningViewModelAsync(int id)
    {
        var sprint = await _context.Sprints
            .Include(x => x.Project)
            .FirstOrDefaultAsync(x => x.Id == id && x.ViewState);

        if (sprint == null)
            return null;

        var backlogStories = await _context.UserStories
            .Where(x => x.ProjectId == sprint.ProjectId && x.SprintId == null && x.ViewState)
            .Include(x => x.Owner)
            .OrderBy(x => x.Order)
            .ToListAsync();

        var sprintStories = await _context.UserStories
            .Where(x => x.SprintId == id && x.ViewState)
            .Include(x => x.Owner)
            .OrderBy(x => x.Order)
            .ToListAsync();

        return new SprintPlanningViewModel
        {
            SprintId = sprint.Id,
            SprintName = sprint.Name,
            ProjectId = sprint.ProjectId,
            ProjectName = sprint.Project.Name,
            Capacity = sprint.Capacity,
            CanManage = await _sprintService.CanManageSprintAsync(id, CurrentUser.UserId),
            BacklogStories = backlogStories.Select(x => new PlanningStoryItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                StoryPoint = x.StoryPoint,
                Priority = x.Priority,
                Status = x.Status,
                OwnerName = x.Owner != null ? x.Owner.FullName : null
            }).ToList(),
            SprintStories = sprintStories.Select(x => new PlanningStoryItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                StoryPoint = x.StoryPoint,
                Priority = x.Priority,
                Status = x.Status,
                OwnerName = x.Owner != null ? x.Owner.FullName : null
            }).ToList()
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignToSprint(int storyId, int sprintId)
    {
        if (!await _sprintService.CanManageSprintAsync(sprintId, CurrentUser.UserId))
            return Forbid();

        await _userStoryService.MoveToSprintAsync(storyId, sprintId);
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveFromSprintPlanning(int storyId, int sprintId)
    {
        if (!await _sprintService.CanManageSprintAsync(sprintId, CurrentUser.UserId))
            return Forbid();

        await _userStoryService.RemoveFromSprintAsync(storyId);
        return Json(new { success = true });
    }
}