using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Backlog;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class BacklogController : BaseController
{
    private readonly IUserStoryService _userStoryService;
    private readonly IBacklogService _backlogService;
    private readonly ApplicationDbContext _context;

    public BacklogController(
        IUserStoryService userStoryService,
        IBacklogService backlogService,
        ICurrentUserService currentUser,
        ApplicationDbContext context)
        : base(currentUser)
    {
        _userStoryService = userStoryService;
        _backlogService = backlogService;
        _context = context;
    }

    public async Task<IActionResult> Index(int projectId)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == projectId);

        if (project == null)
            return NotFound();

        if (!await IsProjectMemberAsync(_context, projectId))
        {
            TempData["Error"] = "شما عضو این پروژه نیستید.";
            return RedirectToAction("Index", "Workspace");
        }

        await _backlogService.GetOrCreateAsync(projectId);

        // Load all sprints for this project
        var sprints = await _context.Sprints
            .Where(x => x.ProjectId == projectId && x.ViewState)
            .Include(x => x.UserStories.Where(s => s.ViewState))
            .ThenInclude(x => x.Owner)
            .OrderBy(x => x.StartDate)
            .ToListAsync();

        // Load unassigned stories (not in any sprint)
        var unassignedStories = await _context.UserStories
            .Where(x => x.ProjectId == projectId && x.SprintId == null && x.ViewState)
            .Include(x => x.Owner)
            .OrderBy(x => x.Order)
            .ToListAsync();

        var members = await _context.ProjectMembers
            .Where(x => x.ProjectId == projectId && x.ViewState)
            .Include(x => x.ApplicationUser)
            .ToListAsync();

        var contributorsMap = await _userStoryService.GetContributorsMapAsync(projectId);

        var vm = new BacklogIndexViewModel
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            CanManage = await _userStoryService.CanManageBacklogAsync(projectId, CurrentUser.UserId),
            Sprints = sprints.Select(sprint => new SprintGroupViewModel
            {
                SprintId = sprint.Id,
                SprintName = sprint.Name,
                StartDate = sprint.StartDate,
                EndDate = sprint.EndDate,
                Status = sprint.Status,
                Stories = sprint.UserStories.Select(x => new UserStoryListItemViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    StoryPoint = x.StoryPoint,
                    BusinessValue = x.BusinessValue,
                    Order = x.Order,
                    Priority = x.Priority,
                    Status = x.Status,
                    OwnerId = x.OwnerId,
                    OwnerName = x.Owner != null ? x.Owner.FullName : null,
                    Contributors = contributorsMap.TryGetValue(x.Id, out var names) ? names : new List<string>()
                }).ToList()
            }).ToList(),
            UnassignedStories = unassignedStories.Select(x => new UserStoryListItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                StoryPoint = x.StoryPoint,
                BusinessValue = x.BusinessValue,
                Order = x.Order,
                Priority = x.Priority,
                Status = x.Status,
                OwnerId = x.OwnerId,
                OwnerName = x.Owner != null ? x.Owner.FullName : null,
                Contributors = contributorsMap.TryGetValue(x.Id, out var names) ? names : new List<string>()
            }).ToList(),
            ProjectMembers = members.Select(x => new ProjectMemberOptionViewModel
            {
                UserId = x.ApplicationUserId,
                FullName = x.ApplicationUser.FullName
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickAdd(CreateQuickStoryViewModel model)
    {
        if (!await _userStoryService.CanManageBacklogAsync(model.ProjectId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه افزودن User Story در این پروژه را ندارید.";
            return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = "عنوان User Story را وارد کنید.";
            return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
        }

        var backlog = await _backlogService.GetOrCreateAsync(model.ProjectId);

        if (await _userStoryService.ExistsByTitleAsync(backlog.Id, model.Title))
        {
            TempData["Error"] = "User Story با این عنوان قبلاً وجود دارد.";
            return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
        }

        var existingStories = await _userStoryService.GetBacklogStoriesAsync(model.ProjectId);
        var nextOrder = existingStories.Any() ? existingStories.Max(x => x.Order) + 1 : 0;

        var story = new UserStory
        {
            ProjectId = model.ProjectId,
            BacklogId = backlog.Id,
            Title = model.Title,
            Order = nextOrder,
            Priority = StoryPriorityType.Medium,
            Status = StoryStatusType.New,
            CreatedDate = DateTime.Now,
            ViewState = true
        };

        await _userStoryService.AddAsync(story);

        TempData["Success"] = "User Story جدید با موفقیت اضافه شد.";
        return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePriority(int storyId, StoryPriorityType priority, int projectId)
    {
        if (!await _userStoryService.CanManageStoryAsync(storyId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه تغییر اولویت این آیتم را ندارید.";
            return RedirectToAction(nameof(Index), new { projectId });
        }

        await _userStoryService.ChangePriorityAsync(storyId, priority);
        return RedirectToAction(nameof(Index), new { projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int storyId, StoryStatusType status, int projectId)
    {
        if (!await _userStoryService.CanManageStoryAsync(storyId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه تغییر وضعیت این آیتم را ندارید.";
            return RedirectToAction(nameof(Index), new { projectId });
        }

        await _userStoryService.ChangeStatusAsync(storyId, status);
        return RedirectToAction(nameof(Index), new { projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeOwner(int storyId, int? ownerId, int projectId)
    {
        if (!await _userStoryService.CanManageStoryAsync(storyId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه تغییر مسئول این آیتم را ندارید.";
            return RedirectToAction(nameof(Index), new { projectId });
        }

        await _userStoryService.ChangeOwnerAsync(storyId, ownerId);
        return RedirectToAction(nameof(Index), new { projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int projectId)
    {
        if (!await _userStoryService.CanManageStoryAsync(id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه حذف این آیتم را ندارید.";
            return RedirectToAction(nameof(Index), new { projectId });
        }

        await _userStoryService.DeleteAsync(id);

        TempData["Success"] = "User Story حذف شد.";
        return RedirectToAction(nameof(Index), new { projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reorder([FromForm] ReorderStoriesViewModel model)
    {
        if (!await _userStoryService.CanManageBacklogAsync(model.ProjectId, CurrentUser.UserId))
            return Forbid();

        await _userStoryService.ReorderAsync(model.OrderedIds);
        return Json(new { success = true });
    }
}