using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class ProjectReportController : BaseController
{
    private readonly IProjectReportService _reportService;
    private readonly IProjectService _projectService;
    private readonly IWorkspaceMemberService _workspaceMemberService;
    private readonly IReportExportService _exportService;

    public ProjectReportController(
        IProjectReportService reportService,
        IProjectService projectService,
        IWorkspaceMemberService workspaceMemberService,
        IReportExportService exportService,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _reportService = reportService;
        _projectService = projectService;
        _workspaceMemberService = workspaceMemberService;
        _exportService = exportService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int projectId, DateTime? fromDate, DateTime? toDate)
    {
        var project = await _projectService.GetByIdAsync(projectId);
        if (project == null)
            return NotFound();

        if (!await _workspaceMemberService.IsMemberAsync(project.WorkspaceId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما عضو این Workspace نیستید.";
            return RedirectToAction("Index", "Workspace");
        }

        var model = await _reportService.GetReportAsync(projectId, fromDate, toDate);
        if (model == null)
            return NotFound();

        ViewBag.WorkspaceId = project.WorkspaceId;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf(int projectId, DateTime? fromDate, DateTime? toDate)
    {
        var model = await _reportService.GetReportAsync(projectId, fromDate, toDate);
        if (model == null)
            return NotFound();

        var bytes = _exportService.GenerateProjectPdf(model);
        return File(bytes, "application/pdf", $"Report-{model.ProjectName}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportExcel(int projectId, DateTime? fromDate, DateTime? toDate)
    {
        var model = await _reportService.GetReportAsync(projectId, fromDate, toDate);
        if (model == null)
            return NotFound();

        var bytes = _exportService.GenerateProjectExcel(model);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Report-{model.ProjectName}.xlsx");
    }
}