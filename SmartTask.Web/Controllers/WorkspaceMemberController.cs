using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Data;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Workspace;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class WorkspaceMemberController : BaseController
{
    private readonly IWorkspaceMemberService _workspaceMemberService;

    public WorkspaceMemberController(
        IWorkspaceMemberService workspaceMemberService,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _workspaceMemberService = workspaceMemberService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int workspaceId)
    {
        var members = await _workspaceMemberService
            .GetMembersAsync(workspaceId);

        ViewBag.WorkspaceId = workspaceId;

        return View(members);
    }

    [HttpGet]
    public IActionResult Invite(int workspaceId)
    {
        var model = new InviteMemberViewModel
        {
            WorkspaceId = workspaceId
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Invite(
        InviteMemberViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        await _workspaceMemberService.InviteMemberAsync(
            model.WorkspaceId,
            model.UserId,
            model.Role);

        TempData["Success"] =
            "عضو جدید با موفقیت اضافه شد.";

        return RedirectToAction(
            nameof(Index),
            new { workspaceId = model.WorkspaceId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int memberId, int workspaceId)
    {
        await _workspaceMemberService
            .RemoveMemberAsync(memberId);

        TempData["Success"] =
            "عضو با موفقیت حذف شد.";

        return RedirectToAction(
            nameof(Index),
            new { workspaceId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(
        int memberId,
        int workspaceId,
        WorkspaceRoleType role)
    {
        await _workspaceMemberService
            .ChangeRoleAsync(memberId, role);

        TempData["Success"] =
            "نقش کاربر بروزرسانی شد.";

        return RedirectToAction(
            nameof(Index),
            new { workspaceId });
    }
}