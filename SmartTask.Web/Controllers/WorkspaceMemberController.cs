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
    private readonly IWorkspaceInvitationService _invitationService;
    private readonly IUserService _userService;
    public WorkspaceMemberController(
        IWorkspaceMemberService workspaceMemberService,
        IWorkspaceInvitationService invitationService,
        IUserService userService,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _workspaceMemberService = workspaceMemberService;
        _invitationService = invitationService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int workspaceId)
    {
        if (!await _workspaceMemberService.IsMemberAsync(workspaceId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما عضو این Workspace نیستید.";
            return RedirectToAction("Index", "Workspace");
        }

        var canManage = await _workspaceMemberService.IsOwnerOrAdminAsync(
            workspaceId, CurrentUser.UserId);

        var model = new WorkspaceMemberIndexViewModel
        {
            WorkspaceId = workspaceId,
            CanManage = canManage,
            Members = await _workspaceMemberService.GetMembersAsync(workspaceId, CurrentUser.UserId),
            Invitations = canManage
                ? await _invitationService.GetPendingInvitationsAsync(workspaceId)
                : new List<WorkspaceInvitationViewModel>()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Invite(int workspaceId)
    {
        if (!await _workspaceMemberService.IsOwnerOrAdminAsync(workspaceId, CurrentUser.UserId))
        {
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = false, message = "شما اجازه دعوت عضو جدید را ندارید." });

            TempData["Error"] = "شما اجازه دعوت عضو جدید را ندارید.";
            return RedirectToAction(nameof(Index), new { workspaceId });
        }

        var model = new InviteMemberViewModel { WorkspaceId = workspaceId };

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return PartialView("_InviteModal", model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Invite(InviteMemberViewModel model)
    {
        var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

        if (!await _workspaceMemberService.IsOwnerOrAdminAsync(model.WorkspaceId, CurrentUser.UserId))
        {
            if (isAjax)
                return Json(new { success = false, message = "شما اجازه دعوت عضو جدید را ندارید." });

            TempData["Error"] = "شما اجازه دعوت عضو جدید را ندارید.";
            return RedirectToAction(nameof(Index), new { workspaceId = model.WorkspaceId });
        }

        if (!model.UserIds.Any() && !model.Emails.Any())
        {
            ModelState.AddModelError("", "حداقل یک کاربر یا ایمیل را انتخاب کنید.");

            if (isAjax)
                return BadRequest(new { success = false, errors = ModelState.ToDictionary(k => k.Key, v => v.Value?.Errors.Select(e => e.ErrorMessage).ToArray()) });

            return View(model);
        }

        try
        {
            await _invitationService.InviteAsync(
                model.WorkspaceId, model.UserIds, model.Emails, model.Role, CurrentUser.UserId);

            if (isAjax)
                return Json(new { success = true, message = "دعوت‌نامه‌ها با موفقیت ارسال شد." });

            TempData["Success"] = "دعوت‌نامه‌ها با موفقیت ارسال شد.";
        }
        catch (Exception ex)
        {
            if (isAjax)
                return Json(new { success = false, message = ex.Message });

            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { workspaceId = model.WorkspaceId });
    }

    [HttpGet]
    public async Task<IActionResult> AcceptInvitation(Guid token)
    {
        try
        {
            await _invitationService.AcceptInvitationAsync(token, CurrentUser.UserId);
            TempData["Success"] = "شما با موفقیت به فضای کاری اضافه شدید.";

            var invitation = await _invitationService.GetByTokenAsync(token);
            return RedirectToAction("Details", "Workspace", new { id = invitation!.WorkspaceId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index", "Workspace");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelInvitation(int invitationId, int workspaceId)
    {
        if (!await _workspaceMemberService.IsOwnerOrAdminAsync(workspaceId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه لغو دعوت‌نامه را ندارید.";
            return RedirectToAction(nameof(Index), new { workspaceId });
        }

        await _invitationService.CancelInvitationAsync(invitationId);
        TempData["Success"] = "دعوت‌نامه لغو شد.";

        return RedirectToAction(nameof(Index), new { workspaceId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(int memberId, int workspaceId)
    {
        if (!await _workspaceMemberService.IsOwnerOrAdminAsync(workspaceId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه حذف عضو را ندارید.";
            return RedirectToAction(nameof(Index), new { workspaceId });
        }

        try
        {
            await _workspaceMemberService.RemoveMemberAsync(memberId);
            TempData["Success"] = "عضو با موفقیت حذف شد.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { workspaceId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(int memberId, int workspaceId, WorkspaceRoleType role)
    {
        if (!await _workspaceMemberService.IsOwnerOrAdminAsync(workspaceId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما اجازه تغییر نقش را ندارید.";
            return RedirectToAction(nameof(Index), new { workspaceId });
        }

        try
        {
            await _workspaceMemberService.ChangeRoleAsync(memberId, role);
            TempData["Success"] = "نقش کاربر بروزرسانی شد.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { workspaceId });
    }

    [HttpGet]
    public async Task<IActionResult> SearchUsers(int workspaceId, string term)
    {
        var result = await _userService.SearchUsersAsync(term, workspaceId);
        return Json(result);
    }
}