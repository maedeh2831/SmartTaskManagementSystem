using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Ai;
using SmartTask.Web.Models.ViewModels.Priority;
using SmartTask.Web.Services.AI;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations;

public class PriorityEngineService : IPriorityEngineService
{
    private readonly ApplicationDbContext _context;
    private readonly ITaskDependencyService _dependencyService;
    private readonly IWorkloadAnalysisService _workloadService;
    private readonly ITaskService _taskService;
    private readonly IAiClientService _aiClient;

    public PriorityEngineService(
        ApplicationDbContext context,
        ITaskDependencyService dependencyService,
        IWorkloadAnalysisService workloadService,
        ITaskService taskService,
        IAiClientService aiClient)
    {
        _context = context;
        _dependencyService = dependencyService;
        _workloadService = workloadService;
        _taskService = taskService;
        _aiClient = aiClient;
    }

    public async Task<SmartPriorityViewModel> GetSuggestionAsync(int taskId, int currentUserId)
    {
        var task = await _context.TaskItems
            .Include(t => t.UserStory)
            .Include(t => t.Assignments)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            throw new InvalidOperationException("Task یافت نشد.");

        var projectId = task.UserStory.ProjectId;
        var reasons = new List<string>();

        // ===== ۱) امتیاز فوریت زمانی (۰-۴۰) =====
        int urgencyScore;

        if (!task.DueDate.HasValue)
        {
            urgencyScore = 10;
            reasons.Add("موعد مشخصی برای این Task ثبت نشده (امتیاز پیش‌فرض).");
        }
        else
        {
            var daysUntilDue = (task.DueDate.Value.Date - DateTime.Now.Date).Days;

            if (daysUntilDue <= 0)
            {
                urgencyScore = 40;
                reasons.Add(daysUntilDue == 0
                    ? "موعد این Task امروز است."
                    : $"موعد این Task {Math.Abs(daysUntilDue)} روز گذشته است.");
            }
            else
            {
                urgencyScore = (int)Math.Clamp(40 - daysUntilDue * (40.0 / 30.0), 0, 40);
                reasons.Add($"{daysUntilDue} روز تا موعد این Task باقی مانده است.");
            }
        }

        // ===== ۲) امتیاز تأثیر وابستگی (۰-۳۵) =====
        var impactedChain = await _dependencyService.GetImpactedTasksAsync(taskId, 0);
        var requiredCount = impactedChain.Count(x => x.IsRequiredChain);
        var dependencyScore = Math.Min(requiredCount * 7, 35);

        if (requiredCount > 0)
            reasons.Add($"تأخیر این Task می‌تواند روی {requiredCount} Task دیگر تأثیر مستقیم بگذارد.");

        // ===== ۳) امتیاز ریسک کارکارگیر (۰-۲۵) =====
        int workloadScore = 0;
        var assigneeIds = task.Assignments.Select(a => a.ApplicationUserId).Distinct().ToList();

        if (assigneeIds.Any())
        {
            var maxUtilization = 0;
            foreach (var userId in assigneeIds)
            {
                var utilization = await _workloadService.GetUserUtilizationAsync(projectId, userId);
                maxUtilization = Math.Max(maxUtilization, utilization);
            }

            if (maxUtilization > 100)
            {
                workloadScore = 25;
                reasons.Add("فرد مسئول این Task در حال حاضر بیش از ظرفیت خود مشغول است.");
            }
            else if (maxUtilization >= 80)
            {
                workloadScore = 15;
                reasons.Add("فرد مسئول این Task نزدیک به ظرفیت کامل است.");
            }
        }

        var totalScore = Math.Clamp(urgencyScore + dependencyScore + workloadScore, 0, 100);

        var suggestedPriority = totalScore switch
        {
            <= 20 => TaskPriorityType.Lowest,
            <= 40 => TaskPriorityType.Low,
            <= 60 => TaskPriorityType.Medium,
            <= 80 => TaskPriorityType.High,
            _ => TaskPriorityType.Highest
        };

        return new SmartPriorityViewModel
        {
            TaskId = taskId,
            CurrentPriority = task.Priority,
            SuggestedPriority = suggestedPriority,
            TotalScore = totalScore,
            UrgencyScore = urgencyScore,
            DependencyScore = dependencyScore,
            WorkloadScore = workloadScore,
            Reasons = reasons,
            CanApply = await _taskService.CanManageTaskAsync(taskId, currentUserId)
        };
    }

    /// <summary>
    /// تحلیل ترکیبی: الگوریتم + LLM. ابتدا امتیاز الگوریتمی،
    /// سپس LLM دلایل تکمیلی و پیشنهاد عملی تولید می‌کنه.
    /// </summary>
    public async Task<SmartPriorityViewModel> GetSuggestionWithAiAsync(int taskId, int currentUserId)
    {
        // 1) تحلیل الگوریتمی (همون کد قبلی)
        var suggestion = await GetSuggestionAsync(taskId, currentUserId);

        // 2) ارسال داده‌ها به LLM برای دلایل تکمیلی
        try
        {
            var task = await _context.TaskItems
                .Include(t => t.UserStory)
                .Include(t => t.Assignments)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task != null)
            {
                var aiResult = await GetAiPriorityReasonsAsync(task, suggestion);
                if (aiResult != null)
                {
                    suggestion.AiReasons = aiResult.AiReasons;
                    suggestion.AiSuggestedAction = aiResult.AiSuggestedAction;
                    suggestion.AiExplanation = aiResult.Explanation;
                    suggestion.AiResourceSuggestion = aiResult.ResourceSuggestion;
                }
            }
        }
        catch
        {
            // اگه LLM fail کرد، فقط تحلیل الگوریتمی نمایش داده میشه
        }

        return suggestion;
    }

    /// <summary>
    /// ارسال داده‌های اولویت به LLM و دریافت دلایل تکمیلی.
    /// </summary>
    private async Task<AiPriorityReasonResult?> GetAiPriorityReasonsAsync(
        Models.Entities.TaskItem task, SmartPriorityViewModel algoResult)
    {
        var systemPrompt =
            "تو یک مشاور مدیریت پروژه نرم‌افزاری هستی. " +
            "بر اساس اطلاعات زیر، دلایل تکمیلی برای اولویت‌بندی این Task به صورت JSON بازگردان.\n" +
            "فقط JSON معتبر برگردان. فرمت:\n" +
            "{\n" +
            "  \"ai_reasons\": [حداکثر 3 دلیل تکمیلی به فارسی],\n" +
            "  \"ai_suggested_action\": \"عمل پیشنهادی به فارسی\",\n" +
            "  \"explanation\": \"توضیح 1-2 جمله‌ای چرا این اولویت پیشنهاد شده\",\n" +
            "  \"resource_suggestion\": \"پیشنهاد در مورد تخصیص منابع\"\n" +
            "}";

        var assignees = string.Join(", ", task.Assignments.Select(a => a.ApplicationUserId));
        var userPrompt =
            $"عنوان Task: {task.Title}\n" +
            $"اولویت فعلی: {algoResult.CurrentPriority}\n" +
            $"اولویت پیشنهادی الگوریتم: {algoResult.SuggestedPriority} (امتیاز: {algoResult.TotalScore}/100)\n" +
            $" форیت زمانی: {algoResult.UrgencyScore}/40\n" +
            $"وابستگی: {algoResult.DependencyScore}/35\n" +
            $"بارکاری: {algoResult.WorkloadScore}/25\n" +
            $"دلایل الگوریتم: {string.Join(" | ", algoResult.Reasons)}\n" +
            $"مسئول(ین): {assignees}";

        return await _aiClient.GetStructuredCompletionAsync<AiPriorityReasonResult>(
            systemPrompt, userPrompt, temperature: 0.5);
    }

    public async Task ApplySuggestionAsync(int taskId, int currentUserId)
    {
        if (!await _taskService.CanManageTaskAsync(taskId, currentUserId))
            throw new UnauthorizedAccessException("شما اجازه ویرایش این Task را ندارید.");

        var suggestion = await GetSuggestionAsync(taskId, currentUserId);
        var task = await _context.TaskItems.FirstOrDefaultAsync(x => x.Id == taskId);
        if (task == null) return;

        task.Priority = suggestion.SuggestedPriority;
        task.ChangeDate = DateTime.Now;
        await _context.SaveChangesAsync();
    }
}