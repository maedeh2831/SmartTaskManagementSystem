using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Health;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations;

public class ProjectHealthService : IProjectHealthService
{
    private readonly ApplicationDbContext _context;
    private readonly IDelayRiskService _delayRiskService;

    public ProjectHealthService(ApplicationDbContext context, IDelayRiskService delayRiskService)
    {
        _context = context;
        _delayRiskService = delayRiskService;
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

        //  سلامت پیشرفت واقعی (همه‌ی تسک‌های ViewState، فارغ از باز/بسته) 
        var allTasks = await _context.TaskItems
            .Where(t => t.UserStory.ProjectId == projectId && t.ViewState)
            .ToListAsync();

        var totalTasksCount = allTasks.Count;
        var completedTasksCount = allTasks.Count(t => t.Status == TaskStatusType.Done);

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
}