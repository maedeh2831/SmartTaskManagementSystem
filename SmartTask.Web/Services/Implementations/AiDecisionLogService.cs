using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Services.Implementations;

/// <summary>
/// سرویس ثبت تصمیمات مدیر در پاسخ به پیشنهادات هوش مصنوعی.
/// برای ارزیابی صحت AI و تحلیل عملکرد سیستم تصمیم‌یار استفاده میشه.
/// </summary>
public class AiDecisionLogService
{
    private readonly ApplicationDbContext _context;

    public AiDecisionLogService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// ثبت پیشنهاد AI و تصمیم مدیر.
    /// </summary>
    public async Task LogDecisionAsync(
        AiDecisionEntityType entityType,
        int entityId,
        AiDecisionType decisionType,
        int? aiScore,
        string? aiSuggestion,
        string? aiReasons,
        AiUserDecision userDecision,
        string? userReason,
        int userId)
    {
        var log = new AiDecisionLog
        {
            EntityType = entityType,
            EntityId = entityId,
            DecisionType = decisionType,
            AiScore = aiScore,
            AiSuggestion = aiSuggestion,
            AiReasons = aiReasons,
            UserDecision = userDecision,
            UserReason = userReason,
            UserId = userId,
            DecisionDate = DateTime.Now,
            ViewState = true
        };

        _context.AiDecisionLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// ثبت پذیرش پیشنهاد اولویت.
    /// </summary>
    public async Task LogPriorityAcceptedAsync(int taskId, int? aiScore, string? aiSuggestion, string? aiReasons, int userId)
    {
        await LogDecisionAsync(
            AiDecisionEntityType.Task, taskId,
            AiDecisionType.Priority, aiScore, aiSuggestion, aiReasons,
            AiUserDecision.Accepted, null, userId);
    }

    /// <summary>
    /// ثبت رد پیشنهاد اولویت.
    /// </summary>
    public async Task LogPriorityRejectedAsync(int taskId, int? aiScore, string? aiSuggestion, string? aiReasons, string? userReason, int userId)
    {
        await LogDecisionAsync(
            AiDecisionEntityType.Task, taskId,
            AiDecisionType.Priority, aiScore, aiSuggestion, aiReasons,
            AiUserDecision.Rejected, userReason, userId);
    }

    /// <summary>
    /// ثبت مشاهده تحلیل ریسک (بدون تصمیم خاص).
    /// </summary>
    public async Task LogRiskViewedAsync(int projectId, int? aiScore, string? aiSuggestion, int userId)
    {
        await LogDecisionAsync(
            AiDecisionEntityType.Project, projectId,
            AiDecisionType.Risk, aiScore, aiSuggestion, null,
            AiUserDecision.Ignored, null, userId);
    }

    /// <summary>
    /// دریافت آمار عملکرد AI برای پروژه.
    /// </summary>
    public async Task<AiPerformanceStats> GetPerformanceStatsAsync(int projectId)
    {
        var logs = await _context.AiDecisionLogs
            .Where(x => x.EntityId == projectId && x.ViewState)
            .ToListAsync();

        return new AiPerformanceStats
        {
            TotalSuggestions = logs.Count,
            Accepted = logs.Count(x => x.UserDecision == AiUserDecision.Accepted),
            Rejected = logs.Count(x => x.UserDecision == AiUserDecision.Rejected),
            Ignored = logs.Count(x => x.UserDecision == AiUserDecision.Ignored)
        };
    }
}

/// <summary>
/// آمار عملکرد سیستم AI.
/// </summary>
public class AiPerformanceStats
{
    public int TotalSuggestions { get; set; }
    public int Accepted { get; set; }
    public int Rejected { get; set; }
    public int Ignored { get; set; }
    public double AcceptanceRate => TotalSuggestions == 0 ? 0 : Math.Round((double)Accepted / TotalSuggestions * 100, 1);
}
