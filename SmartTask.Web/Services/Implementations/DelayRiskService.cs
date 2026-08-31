using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Ai;
using SmartTask.Web.Models.ViewModels.Risk;
using SmartTask.Web.Models.ViewModels.Search;
using SmartTask.Web.Services.AI;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations;

public class DelayRiskService : IDelayRiskService
{
    private readonly ApplicationDbContext _context;
    private readonly IWorkloadAnalysisService _workloadService;
    private readonly ITaskDependencyService _dependencyService;
    private readonly IAiClientService _aiClient;

    public DelayRiskService(
        ApplicationDbContext context,
        IWorkloadAnalysisService workloadService,
        ITaskDependencyService dependencyService,
        IAiClientService aiClient)
    {
        _context = context;
        _workloadService = workloadService;
        _dependencyService = dependencyService;
        _aiClient = aiClient;
    }

    public async Task<DelayRiskViewModel?> GetRiskOverviewAsync(int projectId, int currentUserId)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == projectId && x.ViewState);
        if (project == null)
            return null;

        // OPTIMIZED: Server-side counting instead of loading all tasks into memory
        var now = DateTime.Now;
        var totalOpen = await _context.TaskItems
            .CountAsync(t => t.UserStory.ProjectId == projectId && t.ViewState
                && t.Status != TaskStatusType.Done && t.Status != TaskStatusType.Cancelled);

        var overdueCount = await _context.TaskItems
            .CountAsync(t => t.UserStory.ProjectId == projectId && t.ViewState
                && t.Status != TaskStatusType.Done && t.Status != TaskStatusType.Cancelled
                && t.DueDate.HasValue && t.DueDate.Value.Date < now.Date);

        var overdueRatio = totalOpen == 0 ? 0 : (double)overdueCount / totalOpen;
        var overdueScore = (int)Math.Round(overdueRatio * 40);

        //  ۲) نسبت اعضای اضافه‌بار 
        var workload = await _workloadService.GetWorkloadAsync(projectId, currentUserId);
        var overloadedCount = workload?.ProjectWorkload.Count(x => x.StatusLevel == "overloaded") ?? 0;
        var totalMembers = workload?.ProjectWorkload.Count ?? 0;

        var overloadRatio = totalMembers == 0 ? 0 : (double)overloadedCount / totalMembers;
        var workloadScore = (int)Math.Round(overloadRatio * 30);

        //  ۳) زنجیره‌های پرریسک وابستگی 
        var riskyChains = await _dependencyService.GetProjectRiskOverviewAsync(projectId);
        var dependencyScore = Math.Min(riskyChains.Count * 4, 20);

        //  ۴) فعالیت اخیر Cascade (۷ روز اخیر) 
        var recentCascadeCount = await _context.OverdueCascadeLogs
            .Where(x => x.SourceTask.UserStory.ProjectId == projectId
                && x.AppliedDate >= DateTime.Now.AddDays(-7))
            .CountAsync();

        var cascadeScore = Math.Min(recentCascadeCount * 2, 10);

        var totalScore = Math.Clamp(overdueScore + workloadScore + dependencyScore + cascadeScore, 0, 100);

        var (level, levelDisplay) = totalScore switch
        {
            <= 25 => ("low", "کم"),
            <= 50 => ("medium", "متوسط"),
            <= 75 => ("high", "بالا"),
            _ => ("critical", "بحرانی")
        };

        return new DelayRiskViewModel
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            RiskScore = totalScore,
            RiskLevel = level,
            RiskLevelDisplay = levelDisplay,
            OverdueTasksCount = overdueCount,
            TotalOpenTasksCount = totalOpen,
            OverdueScore = overdueScore,
            OverloadedMembersCount = overloadedCount,
            TotalMembersCount = totalMembers,
            WorkloadScore = workloadScore,
            RiskyDependencyChainsCount = riskyChains.Count,
            DependencyScore = dependencyScore,
            RecentCascadeCount = recentCascadeCount,
            CascadeScore = cascadeScore
        };
    }

    /// <summary>
    /// تحلیل ترکیبی: الگوریتم + LLM. ابتدا امتیاز الگوریتمی محاسبه شده،
    /// سپس داده‌ها به LLM داده میشه و خروجی ساختاریافته JSON دریافت میشه.
    /// </summary>
    public async Task<DelayRiskViewModel?> GetRiskOverviewWithAiAsync(int projectId, int currentUserId)
    {
        // 1) تحلیل الگوریتمی (همون کد قبلی)
        var risk = await GetRiskOverviewAsync(projectId, currentUserId);
        if (risk == null) return null;

        // 2) ارسال داده‌های ساختاریافته به LLM
        try
        {
            var aiResult = await GetAiRiskAnalysisAsync(risk);
            if (aiResult != null)
            {
                risk.AiAnalysis = aiResult.Summary;
                risk.AiFactors = aiResult.Factors;
                risk.AiSuggestion = aiResult.Suggestion;
                risk.AiRiskScore = aiResult.RiskScore;
                risk.AiConfidence = aiResult.Confidence;
            }
        }
        catch (Exception ex)
        {
            // اگه LLM fail کرد، فقط تحلیل الگوریتمی نمایش داده میشه
            System.Diagnostics.Debug.WriteLine($"[DelayRiskService] AI analysis failed: {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null)
                System.Diagnostics.Debug.WriteLine($"[DelayRiskService] Inner: {ex.InnerException.Message}");
        }

        return risk;
    }

    /// <summary>
    /// ارسال داده‌های ریسک به LLM و دریافت خروجی ساختاریافته JSON.
    /// </summary>
    private async Task<AiRiskAnalysisResult?> GetAiRiskAnalysisAsync(DelayRiskViewModel risk)
    {
        var systemPrompt =
            "تو یک تحلیل‌گر مدیریت پروژه نرم‌افزاری هستی. " +
            "بر اساس آمار عددی زیر، تحلیل ریسک تأخیر پروژه را به صورت JSON بازگردان.\n" +
            "فقط JSON معتبر برگردان. فرمت:\n" +
            "{\n" +
            "  \"risk_score\": عدد 0-100,\n" +
            "  \"risk_level\": \"low\" یا \"medium\" یا \"high\" یا \"critical\",\n" +
            "  \"factors\": [لیست عوامل تأثیرگذار به فارسی],\n" +
            "  \"suggestion\": \"پیشنهاد عملی به فارسی\",\n" +
            "  \"confidence\": \"high\" یا \"medium\" یا \"low\",\n" +
            "  \"summary\": \"تحلیل کوتاه 1-2 جمله به فارسی\"\n" +
            "}";

        var userPrompt =
            $"نام پروژه: {risk.ProjectName}\n" +
            $"امتیاز ریسک الگوریتمی: {risk.RiskScore} (سطح: {risk.RiskLevelDisplay})\n" +
            $"تسک‌های عقب‌افتاده: {risk.OverdueTasksCount} از {risk.TotalOpenTasksCount}\n" +
            $"اعضای اضافه‌بار: {risk.OverloadedMembersCount} از {risk.TotalMembersCount}\n" +
            $"زنجیره‌های وابستگی پرریسک: {risk.RiskyDependencyChainsCount}\n" +
            $"تمدید خودکار موعد (۷ روز اخیر): {risk.RecentCascadeCount}\n" +
            $"وزن‌ها → تأخیر: {risk.OverdueScore}/40, بارکاری: {risk.WorkloadScore}/30, وابستگی: {risk.DependencyScore}/20, Cascade: {risk.CascadeScore}/10";

        return await _aiClient.GetStructuredCompletionAsync<AiRiskAnalysisResult>(
            systemPrompt, userPrompt, temperature: 0.5);
    }

    /// <summary>
    /// متد قبلی - برای سازگاری با کدهای موجود حفظ شده.
    /// </summary>
    public async Task<string> GenerateNarrativeAsync(int projectId, int currentUserId)
    {
        var risk = await GetRiskOverviewAsync(projectId, currentUserId);
        if (risk == null)
            throw new InvalidOperationException("پروژه یافت نشد.");

        var systemPrompt =
            "تو یک دستیار تحلیل‌گر مدیریت پروژه نرم‌افزاری هستی. بر اساس آمار عددی که دریافت می‌کنی، " +
            "یک تحلیل کوتاه، روان و حرفه‌ای به زبان فارسی (حداکثر ۴ جمله) درباره وضعیت ریسک تأخیر پروژه بنویس. " +
            "لحن باید مثل یک مشاور مدیریت پروژه باشد؛ نه خیلی رسمی، نه خیلی نگران‌کننده. اگر وضعیت خوب است، تشویق‌کننده باش. " +
            "در پایان یک پیشنهاد عملی مشخص هم بده. فقط متن تحلیل را بازگردان، بدون Markdown و بدون عنوان اضافه.";

        var userPrompt =
            $"نام پروژه: {risk.ProjectName}\n" +
            $"امتیاز کلی ریسک تأخیر: {risk.RiskScore} از ۱۰۰ (سطح: {risk.RiskLevelDisplay})\n" +
            $"تعداد Task های عقب‌افتاده: {risk.OverdueTasksCount} از {risk.TotalOpenTasksCount} Task باز\n" +
            $"اعضای دارای اضافه‌بار کاری: {risk.OverloadedMembersCount} از {risk.TotalMembersCount} عضو\n" +
            $"تعداد زنجیره‌های وابستگی پرریسک: {risk.RiskyDependencyChainsCount}\n" +
            $"تعداد تمدید خودکار موعد در ۷ روز اخیر: {risk.RecentCascadeCount}\n\n" +
            "لطفاً تحلیل ریسک این پروژه را بنویس.";

        return await _aiClient.GetCompletionAsync(systemPrompt, userPrompt, temperature: 0.6);
    }
}