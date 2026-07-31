using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.ViewModels.UserStory;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class UserStoryController : BaseController
{
    private readonly IUserStoryService _userStoryService;
    private readonly IBacklogService _backlogService;
    private readonly ApplicationDbContext _context;

    public UserStoryController(
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

        var stories = await _context.UserStories
            .Where(x => x.ProjectId == projectId && x.ViewState)
            .Include(x => x.Sprint)
            .Include(x => x.Owner)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync();

        var vm = new UserStoryIndexViewModel
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            CanManage = await _userStoryService.CanManageBacklogAsync(projectId, CurrentUser.UserId),
            Stories = stories.Select(x => new UserStoryListItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                StoryPoint = x.StoryPoint,
                BusinessValue = x.BusinessValue,
                Priority = x.Priority,
                Status = x.Status,
                SprintName = x.Sprint != null ? x.Sprint.Name : null,
                OwnerName = x.Owner != null ? x.Owner.FullName : null
            }).ToList()
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var story = await _userStoryService.GetDetailsAsync(id);

        if (story == null)
            return NotFound();

        var owner = story.OwnerId.HasValue
            ? await _context.Users.FirstOrDefaultAsync(x => x.Id == story.OwnerId)
            : null;

        var vm = new UserStoryDetailsViewModel
        {
            Id = story.Id,
            ProjectId = story.ProjectId,
            ProjectName = story.Project.Name,
            Title = story.Title,
            Description = story.Description,
            AcceptanceCriteria = story.AcceptanceCriteria,
            StoryPoint = story.StoryPoint,
            BusinessValue = story.BusinessValue,
            Priority = story.Priority,
            Status = story.Status,
            SprintName = story.Sprint?.Name,
            OwnerName = owner?.FullName,
            CreateDate = story.CreatedDate,
            CanManage = await _userStoryService.CanManageStoryAsync(id, CurrentUser.UserId)
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int projectId)
    {
        if (!await _userStoryService.CanManageBacklogAsync(projectId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ساخت User Story در این پروژه را ندارید.";
            return RedirectToAction(nameof(Index), new { projectId });
        }

        return View(new CreateUserStoryViewModel { ProjectId = projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserStoryViewModel model)
    {
        if (!await _userStoryService.CanManageBacklogAsync(model.ProjectId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ساخت User Story در این پروژه را ندارید.";
            return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
        }

        if (!ModelState.IsValid)
            return View(model);

        var backlog = await _backlogService.GetOrCreateAsync(model.ProjectId);

        if (await _userStoryService.ExistsByTitleAsync(backlog.Id, model.Title))
        {
            ModelState.AddModelError("Title", "User Story با این عنوان قبلاً وجود دارد.");
            return View(model);
        }

        var existingStories = await _userStoryService.GetBacklogStoriesAsync(model.ProjectId);
        var nextOrder = existingStories.Any() ? existingStories.Max(x => x.Order) + 1 : 0;

        var story = new UserStory
        {
            ProjectId = model.ProjectId,
            BacklogId = backlog.Id,
            Title = model.Title,
            Description = model.Description,
            AcceptanceCriteria = model.AcceptanceCriteria,
            StoryPoint = model.StoryPoint,
            BusinessValue = model.BusinessValue,
            Priority = model.Priority,
            Order = nextOrder,
            CreatedDate = DateTime.Now,
            ViewState = true
        };

        await _userStoryService.AddAsync(story);

        TempData["Success"] = "User Story با موفقیت ایجاد شد.";
        return RedirectToAction(nameof(Details), new { id = story.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var story = await _context.UserStories.FirstOrDefaultAsync(x => x.Id == id);

        if (story == null)
            return NotFound();

        if (!await _userStoryService.CanManageStoryAsync(id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ویرایش این User Story را ندارید.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var model = new EditUserStoryViewModel
        {
            Id = story.Id,
            ProjectId = story.ProjectId,
            Title = story.Title,
            Description = story.Description,
            AcceptanceCriteria = story.AcceptanceCriteria,
            StoryPoint = story.StoryPoint,
            BusinessValue = story.BusinessValue,
            Priority = story.Priority,
            Status = story.Status
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserStoryViewModel model)
    {
        if (!await _userStoryService.CanManageStoryAsync(model.Id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ویرایش این User Story را ندارید.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        if (!ModelState.IsValid)
            return View(model);

        var story = await _context.UserStories.FirstOrDefaultAsync(x => x.Id == model.Id);

        if (story == null)
            return NotFound();

        if (await _userStoryService.ExistsByTitleAsync(story.BacklogId, model.Title, model.Id))
        {
            ModelState.AddModelError("Title", "User Story با این عنوان قبلاً وجود دارد.");
            return View(model);
        }

        story.Title = model.Title;
        story.Description = model.Description;
        story.AcceptanceCriteria = model.AcceptanceCriteria;
        story.StoryPoint = model.StoryPoint;
        story.BusinessValue = model.BusinessValue;
        story.Priority = model.Priority;
        story.Status = model.Status;
        story.ChangeDate = DateTime.Now;

        await _context.SaveChangesAsync();

        TempData["Success"] = "User Story با موفقیت ویرایش شد.";
        return RedirectToAction(nameof(Details), new { id = story.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var story = await _context.UserStories.FirstOrDefaultAsync(x => x.Id == id);

        if (story == null)
            return NotFound();

        if (!await _userStoryService.CanManageStoryAsync(id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه حذف این User Story را ندارید.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var projectId = story.ProjectId;

        await _userStoryService.DeleteAsync(id);

        TempData["Success"] = "User Story حذف شد.";
        return RedirectToAction(nameof(Index), new { projectId });
    }
}