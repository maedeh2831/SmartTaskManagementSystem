using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Offroad;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class OffroadController : BaseController
{
    private readonly IOffroadTaskService _offroadTaskService;
    private readonly IProjectMemberService _projectMemberService;
    private readonly ApplicationDbContext _context;

    public OffroadController(
        IOffroadTaskService offroadTaskService,
        IProjectMemberService projectMemberService,
        ICurrentUserService currentUser,
        ApplicationDbContext context)
        : base(currentUser)
    {
        _offroadTaskService = offroadTaskService;
        _projectMemberService = projectMemberService;
        _context = context;
    }

    public async Task<IActionResult> Index(int projectId)
    {
        if (!await _projectMemberService.IsMemberAsync(projectId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما عضو این پروژه نیستید.";
            return RedirectToAction("Index", "Project");
        }

        var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == projectId);
        if (project == null)
            return NotFound();

        var tasks = await _offroadTaskService.GetByProjectAsync(projectId);
        var currentUserId = CurrentUser.UserId;

        var members = await _context.ProjectMembers
            .Where(x => x.ProjectId == projectId && x.ViewState)
            .Include(x => x.ApplicationUser)
            .ToListAsync();

        var vm = new OffroadIndexViewModel
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

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickAdd(CreateOffroadTaskViewModel model)
    {
        if (!await _projectMemberService.IsMemberAsync(model.ProjectId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما عضو این پروژه نیستید.";
            return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
        }

        if (string.IsNullOrWhiteSpace(model.Title))
        {
            TempData["Error"] = "عنوان الزامی است.";
            return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
        }

        var task = new OffroadTask
        {
            ProjectId = model.ProjectId,
            Title = model.Title.Trim(),
            Description = model.Description,
            Priority = model.Priority,
            DueDate = model.DueDate,
            AssignedToUserId = model.AssignedToUserId,
            CreatedByUserId = CurrentUser.UserId,
            Status = OffroadStatusType.ToDo,
            CreatedDate = DateTime.Now,
            ViewState = true
        };

        await _offroadTaskService.AddAsync(task);

        TempData["Success"] = "کار آفرود با موفقیت ثبت شد.";
        return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(int id, int projectId, OffroadStatusType status)
    {
        if (!await _offroadTaskService.CanManageOffroadTaskAsync(id, CurrentUser.UserId))
            return Forbid();

        await _offroadTaskService.ChangeStatusAsync(id, status);
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePriority(int id, int projectId, OffroadPriorityType priority)
    {
        if (!await _offroadTaskService.CanManageOffroadTaskAsync(id, CurrentUser.UserId))
            return Forbid();

        await _offroadTaskService.ChangePriorityAsync(id, priority);
        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int projectId)
    {
        if (!await _offroadTaskService.CanManageOffroadTaskAsync(id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه حذف این آیتم را ندارید.";
            return RedirectToAction(nameof(Index), new { projectId });
        }

        await _offroadTaskService.DeleteAsync(id);

        TempData["Success"] = "با موفقیت حذف شد.";
        return RedirectToAction(nameof(Index), new { projectId });
    }
}