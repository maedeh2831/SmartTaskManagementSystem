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
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartTask.Web.Services.Files;

namespace SmartTask.Web.Controllers;

[Authorize]
public class WorkspaceController : BaseController
{
    private readonly IWorkspaceService _workspaceService;
    private readonly ApplicationDbContext _context;
    private readonly IFileUploadService _fileUploadService;
    private readonly ICurrentContextService _currentContextService;

    public WorkspaceController(
        IWorkspaceService workspaceService,
        ICurrentUserService currentUser,
        ApplicationDbContext context,
        IFileUploadService fileUploadService,
        ICurrentContextService currentContextService)
        : base(currentUser)
    {
        _workspaceService = workspaceService;
        _context = context;
        _fileUploadService = fileUploadService;
        _currentContextService = currentContextService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = CurrentUser.UserId;

        var workspaces = await _context.Workspaces
            .Where(x => x.ViewState
                && x.Members.Any(m => m.ApplicationUserId == userId && m.ViewState))
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
                MembersCount = x.Members.Count(m => m.ViewState),
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

    public async Task<IActionResult> Details(int id)
    {
        var workspace = await _workspaceService.GetDetailsAsync(id);

        if (workspace == null)
            return NotFound();

        if (!await IsWorkspaceMemberAsync(_context, id))
        {
            TempData["Error"] = "شما عضو این فضای کاری نیستید.";
            return RedirectToAction(nameof(Index));
        }

        _currentContextService.SetCurrentWorkspace(id);

        var model = new WorkspaceDetailsViewModel
        {
            Id = workspace.Id,
            Name = workspace.Name,
            Description = workspace.Description,
            Color = workspace.Color,
            Logo = workspace.Logo,
            Visibility = workspace.Visibility,
            CreateDate = workspace.CreateDate,
            OwnerName = workspace.Owner?.FullName ?? "-",
            MembersCount = workspace.Members.Count,
            ProjectsCount = workspace.Projects.Count,
            TasksCount = 0,
            IsOwner = workspace.OwnerId == CurrentUser.UserId
        };


        return View(model);
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

        if (await _workspaceService.ExistsByNameAsync(model.Name))
        {
            ModelState.AddModelError(
                "Name",
                "فضای کاری با این نام قبلاً وجود دارد.");

            return View(model);
        }

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



        TempData["Success"] =
            "فضای کاری با موفقیت ایجاد شد.";


        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(x => x.Id == id);

        if (workspace == null)
            return NotFound();

        if (!await IsWorkspaceMemberAsync(_context, id))
        {
            TempData["Error"] = "شما عضو این فضای کاری نیستید.";
            return RedirectToAction(nameof(Index));
        }

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

        if (await _workspaceService.ExistsByNameAsync(
            model.Name,
            model.Id))
        {
            ModelState.AddModelError(
                "Name",
                "فضای کاری با این نام قبلاً وجود دارد.");

            return View(model);
        }

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
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _workspaceService.IsOwnerAsync(
            id,
            CurrentUser.UserId))
        {
            TempData["Error"] =
                "شما اجازه حذف این Workspace را ندارید.";

            return RedirectToAction(nameof(Index));
        }


        await _workspaceService.DeleteAsync(id);


        TempData["Success"] =
            "Workspace با موفقیت حذف شد.";


        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Settings(int id)
    {
        if (!await IsWorkspaceMemberAsync(_context, id))
        {
            TempData["Error"] = "شما عضو این فضای کاری نیستید.";
            return RedirectToAction(nameof(Index));
        }

        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(x => x.Id == id);

        if (workspace == null)
            return NotFound();

        var model = new WorkspaceSettingsViewModel
        {
            Id = workspace.Id,
            Name = workspace.Name,
            CurrentLogo = workspace.Logo,
            Color = workspace.Color ?? "#4F46E5",
            TimeZone = workspace.TimeZone ?? "Iran Standard Time"
        };

        ViewBag.TimeZones = GetTimeZoneList();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(WorkspaceSettingsViewModel model)
    {
        if (!await _workspaceService.IsOwnerAsync(model.Id, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه دسترسی به تنظیمات این فضای کاری را ندارید.";
            return RedirectToAction(nameof(Index));
        }

        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(x => x.Id == model.Id);

        if (workspace == null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            model.CurrentLogo = workspace.Logo;
            ViewBag.TimeZones = GetTimeZoneList();
            return View(model);
        }

        string? logoPath = null;

        if (model.LogoFile != null)
        {
            logoPath = await _fileUploadService.SaveFileAsync(model.LogoFile, "workspaces");
            _fileUploadService.DeleteFile(workspace.Logo);
        }

        await _workspaceService.UpdateSettingsAsync(
            model.Id,
            logoPath,
            model.Color,
            model.TimeZone);

        TempData["Success"] = "تنظیمات فضای کاری با موفقیت ذخیره شد.";

        return RedirectToAction(nameof(Settings), new { id = model.Id });
    }

    private List<SelectListItem> GetTimeZoneList()
    {
        return TimeZoneInfo.GetSystemTimeZones()
            .Select(tz => new SelectListItem
            {
                Value = tz.Id,
                Text = tz.DisplayName
            })
            .ToList();
    }
}