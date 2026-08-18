using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Priority;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations;

public class PriorityEngineService : IPriorityEngineService
{
    private readonly ApplicationDbContext _context;
    private readonly ITaskDependencyService _dependencyService;
    private readonly IWorkloadAnalysisService _workloadService;
    private readonly ITaskService _taskService;

    public PriorityEngineService(
        ApplicationDbContext context,
        ITaskDependencyService dependencyService,
        IWorkloadAnalysisService workloadService,
        ITaskService taskService)
    {
        _context = context;
        _dependencyService = dependencyService;
        _workloadService = workloadService;
        _taskService = taskService;
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

    // OPTIMIZED: Use ExecuteUpdateAsync instead of load-modify-save
    public async Task ApplySuggestionAsync(int taskId, int currentUserId)
    {
        if (!await _taskService.CanManageTaskAsync(taskId, currentUserId))
            throw new UnauthorizedAccessException("شما اجازه ویرایش این Task را ندارید.");

        var suggestion = await GetSuggestionAsync(taskId, currentUserId);

        await _context.TaskItems
            .Where(x => x.Id == taskId)
            .ExecuteUpdateAsync(u => u
                .SetProperty(x => x.Priority, suggestion.SuggestedPriority)
                .SetProperty(x => x.ChangeDate, DateTime.Now));
    }
}