using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.ViewModels.Label;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class LabelController : BaseController
{
    private readonly ILabelService _labelService;
    private readonly ApplicationDbContext _context;

    public LabelController(
        ILabelService labelService,
        ICurrentUserService currentUser,
        ApplicationDbContext context)
        : base(currentUser)
    {
        _labelService = labelService;
        _context = context;
    }

    public async Task<IActionResult> Index(int projectId)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == projectId);
        if (project == null)
            return NotFound();

        var labels = await _labelService.GetByProjectAsync(projectId);

        var vm = new LabelIndexViewModel
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            CanManage = await _labelService.CanManageLabelsAsync(projectId, CurrentUser.UserId),
            Labels = labels.Select(x => new LabelListItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Color = x.Color
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateLabelViewModel model)
    {
        if (!await _labelService.CanManageLabelsAsync(model.ProjectId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه ساخت Label در این پروژه را ندارید.";
            return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
        }

        if (string.IsNullOrWhiteSpace(model.Name))
        {
            TempData["Error"] = "نام Label را وارد کنید.";
            return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
        }

        if (await _labelService.ExistsByNameAsync(model.ProjectId, model.Name))
        {
            TempData["Error"] = "Label ای با این نام قبلاً وجود دارد.";
            return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
        }

        await _labelService.CreateOrReactivateAsync(
            model.ProjectId,
            model.Name.Trim(),
            string.IsNullOrWhiteSpace(model.Color) ? "#2196F3" : model.Color);

        TempData["Success"] = "Label با موفقیت ایجاد شد.";
        return RedirectToAction(nameof(Index), new { projectId = model.ProjectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int projectId)
    {
        if (!await _labelService.CanManageLabelsAsync(projectId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه حذف این Label را ندارید.";
            return RedirectToAction(nameof(Index), new { projectId });
        }

        await _labelService.DeleteAsync(id);

        TempData["Success"] = "Label حذف شد.";
        return RedirectToAction(nameof(Index), new { projectId });
    }
}