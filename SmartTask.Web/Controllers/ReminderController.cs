using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.ViewModels.Reminder;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class ReminderController : BaseController
{
    private readonly IReminderService _reminderService;

    public ReminderController(IReminderService reminderService, ICurrentUserService currentUser)
        : base(currentUser)
    {
        _reminderService = reminderService;
    }

    public async Task<IActionResult> Index()
    {
        var reminders = await _reminderService.GetByUserAsync(CurrentUser.UserId);

        var items = reminders.Select(x => new ReminderListItemViewModel
        {
            Id = x.Id,
            Title = x.Title,
            ReminderDate = x.ReminderDate,
            IsSent = x.IsSent,
            TaskItemId = x.TaskItemId,
            TaskTitle = x.TaskItem.Title
        }).ToList();

        var model = new ReminderIndexViewModel
        {
            UpcomingReminders = items.Where(x => !x.IsPast).OrderBy(x => x.ReminderDate).ToList(),
            PastReminders = items.Where(x => x.IsPast).OrderByDescending(x => x.ReminderDate).ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> Calendar()
    {
        var reminders = await _reminderService.GetByUserAsync(CurrentUser.UserId);

        var model = new ReminderCalendarViewModel
        {
            Reminders = reminders.Select(x => new ReminderListItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                ReminderDate = x.ReminderDate,
                IsSent = x.IsSent,
                TaskItemId = x.TaskItemId,
                TaskTitle = x.TaskItem.Title
            }).ToList()
        };

        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        var tasks = await _reminderService.GetAssignedTasksAsync(CurrentUser.UserId);

        var model = new CreateReminderViewModel
        {
            ReminderDate = DateTime.Now.AddHours(1),
            AvailableTasks = tasks.Select(x => new TaskOptionViewModel { Id = x.Id, Title = x.Title }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateReminderViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var tasks = await _reminderService.GetAssignedTasksAsync(CurrentUser.UserId);
            model.AvailableTasks = tasks.Select(x => new TaskOptionViewModel { Id = x.Id, Title = x.Title }).ToList();
            return View(model);
        }

        await _reminderService.CreateAsync(model.TaskItemId, CurrentUser.UserId, model.Title, model.ReminderDate);
        TempData["Success"] = "یادآوری با موفقیت ثبت شد.";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!await _reminderService.CanManageReminderAsync(id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ویرایش این یادآوری را ندارید.";
            return RedirectToAction("Index");
        }

        var reminder = await _reminderService.GetByIdAsync(id);
        if (reminder == null)
            return NotFound();

        var model = new EditReminderViewModel
        {
            Id = reminder.Id,
            Title = reminder.Title,
            ReminderDate = reminder.ReminderDate,
            TaskTitle = reminder.TaskItem.Title
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditReminderViewModel model)
    {
        if (!await _reminderService.CanManageReminderAsync(model.Id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ویرایش این یادآوری را ندارید.";
            return RedirectToAction("Index");
        }

        if (!ModelState.IsValid)
            return View(model);

        await _reminderService.UpdateAsync(model.Id, model.Title, model.ReminderDate);
        TempData["Success"] = "یادآوری با موفقیت ویرایش شد.";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _reminderService.CanManageReminderAsync(id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه حذف این یادآوری را ندارید.";
            return RedirectToAction("Index");
        }

        await _reminderService.DeleteAsync(id);
        TempData["Success"] = "یادآوری حذف شد.";
        return RedirectToAction("Index");
    }
}