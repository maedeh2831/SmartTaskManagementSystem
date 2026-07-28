using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.ViewModels.Workspace;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class WorkspaceController : BaseController
{
    private readonly IWorkspaceService _workspaceService;

    public WorkspaceController(
        IWorkspaceService workspaceService,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _workspaceService = workspaceService;
    }

    public IActionResult Index()
    {
        return View();
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
    public IActionResult Create(CreateWorkspaceViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(EditWorkspaceViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Delete(int id)
    {
        return RedirectToAction(nameof(Index));
    }
}