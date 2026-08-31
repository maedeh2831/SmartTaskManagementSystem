using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class DelayRiskController : BaseController
{
    private readonly IDelayRiskService _delayRiskService;
    private readonly IProjectMemberService _projectMemberService;

    public DelayRiskController(
        IDelayRiskService delayRiskService,
        IProjectMemberService projectMemberService,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _delayRiskService = delayRiskService;
        _projectMemberService = projectMemberService;
    }

    public async Task<IActionResult> Index(int projectId)
    {
        if (!await _projectMemberService.IsMemberAsync(projectId, CurrentUser.UserId))
        {
            TempData["Error"] = "شما عضو این پروژه نیستید.";
            return RedirectToAction("Index", "Project");
        }

        var vm = await _delayRiskService.GetRiskOverviewWithAiAsync(projectId, CurrentUser.UserId);

        if (vm == null)
            return NotFound();

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateNarrative(int projectId)
    {
        if (!await _projectMemberService.IsMemberAsync(projectId, CurrentUser.UserId))
            return Json(new { success = false, message = "شما عضو این پروژه نیستید." });

        try
        {
            var narrative = await _delayRiskService.GenerateNarrativeAsync(projectId, CurrentUser.UserId);
            return Json(new { success = true, narrative });
        }
        catch (Exception)
        {
            return Json(new { success = false, message = "خطا در ارتباط با سرویس هوش مصنوعی. لطفاً بعداً دوباره تلاش کنید." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GetAiAnalysis(int projectId)
    {
        if (!await _projectMemberService.IsMemberAsync(projectId, CurrentUser.UserId))
            return Json(new { success = false, message = "شما عضو این پروژه نیستید." });

        try
        {
            var vm = await _delayRiskService.GetRiskOverviewWithAiAsync(projectId, CurrentUser.UserId);
            if (vm == null)
                return Json(new { success = false, message = "پروژه یافت نشد." });

            return Json(new
            {
                success = true,
                aiRiskScore = vm.AiRiskScore,
                aiConfidence = vm.AiConfidence,
                aiFactors = vm.AiFactors,
                aiSuggestion = vm.AiSuggestion,
                aiAnalysis = vm.AiAnalysis,
                algorithmScore = vm.RiskScore
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "خطا در تحلیل هوش مصنوعی: " + ex.Message });
        }
    }
}