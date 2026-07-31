using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Task;
using SmartTask.Web.Services.Interfaces;
using TaskEntity = SmartTask.Web.Models.Entities.TaskItem;

namespace SmartTask.Web.Controllers;

[Authorize]
public class TaskController : BaseController
{
    private readonly ITaskService _taskService;
    private readonly IUserStoryService _userStoryService;
    private readonly ISubTaskService _subTaskService;
    private readonly ITaskAssignmentService _taskAssignmentService;
    private readonly ApplicationDbContext _context;

    public TaskController(
        ITaskService taskService,
        IUserStoryService userStoryService,
        ISubTaskService subTaskService,
        ITaskAssignmentService taskAssignmentService,
        ICurrentUserService currentUser,
        ApplicationDbContext context)
        : base(currentUser)
    {
        _taskService = taskService;
        _userStoryService = userStoryService;
        _subTaskService = subTaskService;
        _taskAssignmentService = taskAssignmentService;
        _context = context;
    }

    public async Task<IActionResult> Index(int userStoryId)
    {
        var story = await _context.UserStories
            .FirstOrDefaultAsync(x => x.Id == userStoryId && x.ViewState);

        if (story == null)
            return NotFound();

        var tasks = await _taskService.GetByUserStoryAsync(userStoryId);

        var vm = new TaskIndexViewModel
        {
            UserStoryId = userStoryId,
            UserStoryTitle = story.Title,
            ProjectId = story.ProjectId,
            CanManage = await _userStoryService.CanManageStoryAsync(userStoryId, CurrentUser.UserId),
            Tasks = tasks.Select(x => new TaskListItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Status = x.Status,
                Priority = x.Priority,
                Type = x.Type,
                Estimate = x.Estimate,
                DueDate = x.DueDate
            }).ToList()
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(int id)
    {
        var task = await _taskService.GetDetailsAsync(id);

        if (task == null)
            return NotFound();

        var subTasks = await _subTaskService.GetByTaskAsync(id);
        var assignees = await _taskAssignmentService.GetAssigneesAsync(id);

        var projectMembers = await _context.ProjectMembers
            .Where(x => x.ProjectId == task.UserStory.ProjectId && x.ViewState)
            .Include(x => x.ApplicationUser)
            .ToListAsync();

        var assignedIds = assignees.Select(x => x.Id).ToHashSet();

        var vm = new TaskDetailsViewModel
        {
            Id = task.Id,
            UserStoryId = task.UserStoryId,
            UserStoryTitle = task.UserStory.Title,
            ProjectId = task.UserStory.ProjectId,
            ProjectName = task.UserStory.Project.Name,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            Type = task.Type,
            Estimate = task.Estimate,
            StartDate = task.StartDate,
            DueDate = task.DueDate,
            CompletedDate = task.CompletedDate,
            CreateDate = task.CreatedDate,
            CanManage = await _taskService.CanManageTaskAsync(id, CurrentUser.UserId),
            SubTasks = subTasks.Select(x => new SubTaskItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                IsCompleted = x.IsCompleted
            }).ToList(),
            Assignees = assignees.Select(x => new AssigneeOptionViewModel
            {
                UserId = x.Id,
                FullName = x.FullName
            }).ToList(),
            AvailableMembers = projectMembers
                .Where(x => !assignedIds.Contains(x.ApplicationUserId))
                .Select(x => new AssigneeOptionViewModel
                {
                    UserId = x.ApplicationUserId,
                    FullName = x.ApplicationUser.FullName
                }).ToList()
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int userStoryId)
    {
        if (!await _userStoryService.CanManageStoryAsync(userStoryId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ساخت Task در این User Story را ندارید.";
            return RedirectToAction(nameof(Index), new { userStoryId });
        }

        return View(new CreateTaskViewModel { UserStoryId = userStoryId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTaskViewModel model)
    {
        if (!await _userStoryService.CanManageStoryAsync(model.UserStoryId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ساخت Task در این User Story را ندارید.";
            return RedirectToAction(nameof(Index), new { userStoryId = model.UserStoryId });
        }

        if (!ModelState.IsValid)
            return View(model);

        if (await _taskService.ExistsByTitleAsync(model.UserStoryId, model.Title))
        {
            ModelState.AddModelError("Title", "Task ای با این عنوان قبلاً وجود دارد.");
            return View(model);
        }

        var task = new TaskEntity
        {
            UserStoryId = model.UserStoryId,
            Title = model.Title,
            Description = model.Description,
            Type = model.Type,
            Priority = model.Priority,
            Estimate = model.Estimate,
            StartDate = model.StartDate,
            DueDate = model.DueDate,
            Status = TaskStatusType.ToDo,
            CreatedDate = DateTime.Now,
            ViewState = true
        };

        await _taskService.AddAsync(task);

        TempData["Success"] = "Task با موفقیت ایجاد شد.";
        return RedirectToAction(nameof(Details), new { id = task.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var task = await _context.TaskItems.FirstOrDefaultAsync(x => x.Id == id);

        if (task == null)
            return NotFound();

        if (!await _taskService.CanManageTaskAsync(id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ویرایش این Task را ندارید.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var model = new EditTaskViewModel
        {
            Id = task.Id,
            UserStoryId = task.UserStoryId,
            Title = task.Title,
            Description = task.Description,
            Type = task.Type,
            Priority = task.Priority,
            Status = task.Status,
            Estimate = task.Estimate,
            StartDate = task.StartDate,
            DueDate = task.DueDate
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditTaskViewModel model)
    {
        if (!await _taskService.CanManageTaskAsync(model.Id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ویرایش این Task را ندارید.";
            return RedirectToAction(nameof(Details), new { id = model.Id });
        }

        if (!ModelState.IsValid)
            return View(model);

        var task = await _context.TaskItems.FirstOrDefaultAsync(x => x.Id == model.Id);

        if (task == null)
            return NotFound();

        if (await _taskService.ExistsByTitleAsync(task.UserStoryId, model.Title, model.Id))
        {
            ModelState.AddModelError("Title", "Task ای با این عنوان قبلاً وجود دارد.");
            return View(model);
        }

        task.Title = model.Title;
        task.Description = model.Description;
        task.Type = model.Type;
        task.Priority = model.Priority;
        task.Status = model.Status;
        task.Estimate = model.Estimate;
        task.StartDate = model.StartDate;
        task.DueDate = model.DueDate;
        task.ChangeDate = DateTime.Now;

        if (model.Status == TaskStatusType.Done && task.CompletedDate == null)
            task.CompletedDate = DateTime.Now;
        else if (model.Status != TaskStatusType.Done)
            task.CompletedDate = null;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Task با موفقیت ویرایش شد.";
        return RedirectToAction(nameof(Details), new { id = task.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _context.TaskItems.FirstOrDefaultAsync(x => x.Id == id);

        if (task == null)
            return NotFound();

        if (!await _taskService.CanManageTaskAsync(id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه حذف این Task را ندارید.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var userStoryId = task.UserStoryId;

        await _taskService.DeleteAsync(id);

        TempData["Success"] = "Task حذف شد.";
        return RedirectToAction(nameof(Index), new { userStoryId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int taskId, TaskStatusType status)
    {
        if (!await _taskService.CanManageTaskAsync(taskId, CurrentUser.UserId))
            return Forbid();

        await _taskService.ChangeStatusAsync(taskId, status);
        return Json(new { success = true });
    }
}