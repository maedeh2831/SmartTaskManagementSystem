using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Ai;
using SmartTask.Web.Models.ViewModels.Health;
using SmartTask.Web.Services.AI;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations;

public class ProjectHealthService : IProjectHealthService
{
    private readonly ApplicationDbContext _context;
    private readonly IDelayRiskService _delayRiskService;
    private readonly IAiClientService _aiClient;

    public ProjectHealthService(
        ApplicationDbContext context,
        IDelayRiskService delayRiskService,
        IAiClientService aiClient)
    {
        _context = context;
        _delayRiskService = delayRiskService;
        _aiClient = aiClient;
    }

    public async Task<ProjectHealthViewModel?> GetHealthAsync(int projectId, int currentUserId)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == projectId && x.ViewState);
        if (project == null)
            return null;

        var risk = await _delayRiskService.GetRiskOverviewAsync(projectId, currentUserId);

        var overdueRatio = risk == null || risk.TotalOpenTasksCount == 0
            ? 0 : (double)risk.OverdueTasksCount / risk.TotalOpenTasksCount;
        var scheduleHealth = (int)Math.Round(100 - overdueRatio * 100);

        var overloadRatio = risk == null || risk.TotalMembersCount == 0
            ? 0 : (double)risk.OverloadedMembersCount / risk.TotalMembersCount;
        var workloadHealth = (int)Math.Round(100 - overloadRatio * 100);

        var dependencyHealth = risk == null
            ? 100 : Math.Max(0, 100 - Math.Min(risk.RiskyDependencyChainsCount * 10, 100));

        // OPTIMIZED: Server-side counting instead of loading all tasks into memory
        var totalTasksCount = await _context.TaskItems
            .CountAsync(t => t.UserStory.ProjectId == projectId && t.ViewState);

        var completedTasksCount = await _context.TaskItems
            .CountAsync(t => t.UserStory.ProjectId == projectId && t.ViewState && t.Status == TaskStatusType.Done);

        var deliveryHealth = totalTasksCount == 0
            ? 100 : (int)Math.Round((double)completedTasksCount / totalTasksCount * 100);

        var healthScore = (int)Math.Clamp(
            scheduleHealth * 0.30 + workloadHealth * 0.25 + dependencyHealth * 0.20 + deliveryHealth * 0.25,
            0, 100);

        var (level, levelDisplay, icon) = healthScore switch
        {
            >= 85 => ("excellent", "عالی", "fa-solid fa-face-smile-beam"),
            >= 70 => ("good", "خوب", "fa-solid fa-face-smile"),
            >= 50 => ("fair", "نیازمند توجه", "fa-solid fa-face-meh"),
            _ => ("poor", "بحرانی", "fa-solid fa-face-frown")
        };

        return new ProjectHealthViewModel
        {
            ProjectId = projectId,
            HealthScore = healthScore,
            HealthLevel = level,
            HealthLevelDisplay = levelDisplay,
            HealthIcon = icon,
            ScheduleHealth = Math.Clamp(scheduleHealth, 0, 100),
            WorkloadHealth = Math.Clamp(workloadHealth, 0, 100),
            DependencyHealth = dependencyHealth,
            DeliveryHealth = deliveryHealth,
            CompletedTasksCount = completedTasksCount,
            TotalTasksCount = totalTasksCount
        };
    }

    /// <summary>
    /// تحلیل ترکیبی: الگوریتم + LLM. ابتدا امتیاز الگوریتمی،
    /// سپس LLM تحلیل تکمیلی تولید می‌کنه.
    /// </summary>
    public async Task<ProjectHealthViewModel?> GetHealthWithAiAsync(int projectId, int currentUserId)
    {
        // 1) تحلیل الگوریتمی (همون کد قبلی)
        var health = await GetHealthAsync(projectId, currentUserId);
        if (health == null) return null;

        // 2) ارسال داده‌ها به LLM
        try
        {
            var aiResult = await GetAiHealthAnalysisAsync(health);
            if (aiResult != null)
            {
                health.AiOverallAssessment = aiResult.OverallAssessment;
                health.AiCriticalAreas = aiResult.CriticalAreas;
                health.AiRecommendations = aiResult.Recommendations;
                health.AiForecast = aiResult.Forecast;
                health.AiActionItems = aiResult.ActionItems;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProjectHealthService] AI analysis failed: {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null)
                System.Diagnostics.Debug.WriteLine($"[ProjectHealthService] Inner: {ex.InnerException.Message}");
        }

        return health;
    }

    /// <summary>
    /// ارسال داده‌های سلامت پروژه به LLM و دریافت تحلیل ساختاریافته.
    /// </summary>
    private async Task<AiHealthAnalysisResult?> GetAiHealthAnalysisAsync(ProjectHealthViewModel health)
    {
        var systemPrompt =
            "تو یک تحلیل‌گر مدیریت پروژه نرم‌افزاری هستی. " +
            "بر اساس شاخص‌های سلامت پروژه، تحلیل جامع به صورت JSON بازگردان.\n" +
            "فقط JSON معتبر برگردان. فرمت:\n" +
            "{\n" +
            "  \"overall_assessment\": \"ارزیابی کلی 1-2 جمله به فارسی\",\n" +
            "  \"critical_areas\": [بخش‌های بحرانی به فارسی],\n" +
            "  \"recommendations\": [حداکثر 3 پیشنهاد عملی به فارسی],\n" +
            "  \"forecast\": \"پیش‌بینی آینده پروژه 1 جمله\",\n" +
            "  \"action_items\": [اولویت اقدامات به فارسی]\n" +
            "}";

        var userPrompt =
            $"امتیاز سلامت کلی: {health.HealthScore}/100 (سطح: {health.HealthLevelDisplay})\n" +
            $"برنامه‌ریزی: {health.ScheduleHealth}/100\n" +
            $"بارکاری: {health.WorkloadHealth}/100\n" +
            $"وابستگی: {health.DependencyHealth}/100\n" +
            $"تحویل: {health.DeliveryHealth}/100\n" +
            $"تسک‌های تکمیل‌شده: {health.CompletedTasksCount} از {health.TotalTasksCount}";

        return await _aiClient.GetStructuredCompletionAsync<AiHealthAnalysisResult>(
            systemPrompt, userPrompt, temperature: 0.5);
    }
}