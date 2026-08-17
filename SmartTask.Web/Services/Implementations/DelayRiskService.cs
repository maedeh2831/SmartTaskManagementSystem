using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Enums;
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

        //  ۱) نسبت Task های عقب‌افتاده 
        var openTasks = await _context.TaskItems
            .Where(t => t.UserStory.ProjectId == projectId && t.ViewState
                && t.Status != TaskStatusType.Done && t.Status != TaskStatusType.Cancelled)
            .ToListAsync();

        var overdueCount = openTasks.Count(t => t.DueDate.HasValue && t.DueDate.Value.Date < DateTime.Now.Date);
        var totalOpen = openTasks.Count;

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