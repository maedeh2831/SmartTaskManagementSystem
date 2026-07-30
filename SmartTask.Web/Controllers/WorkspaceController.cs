using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Workspace;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class WorkspaceController : BaseController
{
    public WorkspaceController(
     IWorkspaceService workspaceService,
     ICurrentUserService currentUser,
     ApplicationDbContext context)
     : base(currentUser)
    {
        _workspaceService = workspaceService;
        _context = context;
    }

    private readonly ApplicationDbContext _context;

    private readonly IWorkspaceService _workspaceService;

    public async Task<IActionResult> Index()
    {
        var workspaces = await _context.Workspaces
            .Include(x => x.Members)
            .Include(x => x.Projects)
            .OrderByDescending(x => x.CreateDate)
            .Select(x => new WorkspaceListItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                Color = x.Color ?? "#4F46E5",
                Logo = x.Logo,
                Visibility = x.Visibility == VisibilityType.Private
                                ? "خصوصی"
                                : "عمومی",
                MembersCount = x.Members.Count,
                ProjectsCount = x.Projects.Count,
                CreateDate = x.CreateDate
            })
            .ToListAsync();

        var vm = new WorkspaceIndexViewModel
        {
            Workspaces = workspaces
        };

        return View(vm);
    }

    public IActionResult Details(int id)
    {
        return View();
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateWorkspaceViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var workspace = new Workspace
        {
            Name = model.Name,
            Description = model.Description,
            Color = model.Color,
            Visibility = model.Visibility,

            OwnerId = CurrentUser.UserId,

            CreateDate = DateTime.Now,

            IsActive = true,

            ViewState = true
        };

        await _workspaceService.AddAsync(workspace);

        var ownerMember = new WorkspaceMember
        {
            WorkspaceId = workspace.Id,

            ApplicationUserId = CurrentUser.UserId,

            Role = WorkspaceRoleType.Owner
        };

        _context.WorkspaceMembers.Add(ownerMember);

        await _context.SaveChangesAsync();

        TempData["Success"] = "فضای کاری با موفقیت ایجاد شد.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(x => x.Id == id);

        if (workspace == null)
            return NotFound();

        var model = new EditWorkspaceViewModel
        {
            Id = workspace.Id,
            Name = workspace.Name,
            Description = workspace.Description,
            Color = workspace.Color,
            Visibility = workspace.Visibility
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditWorkspaceViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(x => x.Id == model.Id);

        if (workspace == null)
            return NotFound();

        workspace.Name = model.Name;
        workspace.Description = model.Description;
        workspace.Color = model.Color;
        workspace.Visibility = model.Visibility;

        await _context.SaveChangesAsync();

        TempData["Success"] = "فضای کاری با موفقیت ویرایش شد.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        return RedirectToAction(nameof(Index));
    }
}