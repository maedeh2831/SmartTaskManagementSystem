using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class SprintReportController : BaseController
{
    private readonly ISprintReportAiService _sprintReportAiService;
    private readonly ISprintService _sprintService;

    public SprintReportController(
        ISprintReportAiService sprintReportAiService,
        ISprintService sprintService,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _sprintReportAiService = sprintReportAiService;
        _sprintService = sprintService;
    }

    [HttpGet]
    public async Task<IActionResult> GetReports(int sprintId)
    {
        var sprint = await _sprintService.GetByIdAsync(sprintId);
        if (sprint == null)
            return NotFound();

        if (!await _sprintService.CanManageSprintAsync(sprintId, CurrentUser.UserId))
            return Forbid();

        var reports = await _sprintReportAiService.GetReportsAsync(sprintId);
        return Json(new { success = true, reports });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Generate(int sprintId)
    {
        if (!await _sprintService.CanManageSprintAsync(sprintId, CurrentUser.UserId))
            return Json(new { success = false, message = "شما اجازه تولید گزارش برای این اسپرینت را ندارید." });

        try
        {
            var report = await _sprintReportAiService.GenerateReportAsync(sprintId, CurrentUser.UserId);
            return Json(new { success = true, report });
        }
        catch (Exception)
        {
            return Json(new { success = false, message = "خطا در ارتباط با سرویس هوش مصنوعی. لطفاً بعداً دوباره تلاش کنید." });
        }
    }
}