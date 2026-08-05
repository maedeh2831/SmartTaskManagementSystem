using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class WorkspaceReportController : BaseController
{
    private readonly IWorkspaceReportService _reportService;
    private readonly IWorkspaceMemberService _workspaceMemberService;
    private readonly IReportExportService _exportService;

    public WorkspaceReportController(
        IWorkspaceReportService reportService,
        IWorkspaceMemberService workspaceMemberService,
        IReportExportService exportService,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _reportService = reportService;
        _workspaceMemberService = workspaceMemberService;
        _exportService = exportService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int workspaceId, DateTime? fromDate, DateTime? toDate)
    {
        if (!await _workspaceMemberService.IsMemberAsync(workspaceId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما عضو این Workspace نیستید.";
            return RedirectToAction("Index", "Workspace");
        }

        try
        {
            var model = await _reportService.GetReportAsync(workspaceId, fromDate, toDate);
            return View(model);
        }
        catch (Exception)
        {
            TempData["Error"] = "فضای کاری یافت نشد.";
            return RedirectToAction("Index", "Workspace");
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf(int workspaceId, DateTime? fromDate, DateTime? toDate)
    {
        if (!await _workspaceMemberService.IsMemberAsync(workspaceId, CurrentUser.UserId))
            return Forbid();

        var model = await _reportService.GetReportAsync(workspaceId, fromDate, toDate);
        var bytes = _exportService.GenerateWorkspacePdf(model);
        return File(bytes, "application/pdf", $"Report-{model.WorkspaceName}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportExcel(int workspaceId, DateTime? fromDate, DateTime? toDate)
    {
        if (!await _workspaceMemberService.IsMemberAsync(workspaceId, CurrentUser.UserId))
            return Forbid();

        var model = await _reportService.GetReportAsync(workspaceId, fromDate, toDate);
        var bytes = _exportService.GenerateWorkspaceExcel(model);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Report-{model.WorkspaceName}.xlsx");
    }
}