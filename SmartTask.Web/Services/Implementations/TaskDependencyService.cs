using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Dependency;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations;

public class TaskDependencyService : ITaskDependencyService
{
    private readonly ApplicationDbContext _context;
    private readonly ITaskService _taskService;

    public TaskDependencyService(ApplicationDbContext context, ITaskService taskService)
    {
        _context = context;
        _taskService = taskService;
    }

    public async Task<TaskDependencyWidgetViewModel> GetWidgetAsync(int taskId, int currentUserId)
    {
        var task = await _context.TaskItems
            .Include(t => t.UserStory)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            throw new InvalidOperationException("Task یافت نشد.");

        var projectId = task.UserStory.ProjectId;

        var dependsOnRaw = await _context.TaskDependencies
            .Where(d => d.TaskItemId == taskId)
            .Include(d => d.DependsOnTaskItem)
            .ToListAsync();

        var dependentsRaw = await _context.TaskDependencies
            .Where(d => d.DependsOnTaskItemId == taskId)
            .Include(d => d.TaskItem)
            .ToListAsync();

        var linkedIds = dependsOnRaw.Select(d => d.DependsOnTaskItemId).ToHashSet();
        linkedIds.Add(taskId);

        var availableTasks = await _context.TaskItems
            .Where(t => t.UserStory.ProjectId == projectId && t.ViewState && !linkedIds.Contains(t.Id))
            .OrderBy(t => t.Title)
            .Select(t => new { t.Id, t.Title })
            .ToListAsync();

        var delayDays = CalculateDelayDays(task);
        var impacted = await GetImpactedTasksAsync(taskId, delayDays);

        return new TaskDependencyWidgetViewModel
        {
            TaskId = taskId,
            CanManage = await _taskService.CanManageTaskAsync(taskId, currentUserId),
            DependsOn = dependsOnRaw.Select(d => new DependencyItemViewModel
            {
                Id = d.Id,
                TaskId = d.DependsOnTaskItemId,
                TaskTitle = d.DependsOnTaskItem.Title,
                TaskStatus = d.DependsOnTaskItem.Status,
                IsRequired = d.IsRequired
            }).ToList(),
            Dependents = dependentsRaw.Select(d => new DependencyItemViewModel
            {
                Id = d.Id,
                TaskId = d.TaskItemId,
                TaskTitle = d.TaskItem.Title,
                TaskStatus = d.TaskItem.Status,
                IsRequired = d.IsRequired
            }).ToList(),
            AvailableTasks = availableTasks
                .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Title })
                .ToList(),
            DelayDays = delayDays,
            ImpactedTasks = impacted
        };
    }

    public async Task<(bool success, string? error)> AddDependencyAsync(int taskId, int dependsOnTaskId, bool isRequired)
    {
        if (taskId == dependsOnTaskId)
            return (false, "یک Task نمی‌تواند به خودش وابسته باشد.");

        var exists = await _context.TaskDependencies
            .AnyAsync(d => d.TaskItemId == taskId && d.DependsOnTaskItemId == dependsOnTaskId);

        if (exists)
            return (false, "این وابستگی قبلاً ثبت شده است.");

        if (await WouldCreateCycleAsync(taskId, dependsOnTaskId))
            return (false, "این وابستگی باعث ایجاد یک چرخه (Cycle) می‌شود و مجاز نیست.");

        _context.TaskDependencies.Add(new TaskDependency
        {
            TaskItemId = taskId,
            DependsOnTaskItemId = dependsOnTaskId,
            IsRequired = isRequired,
            CreatedDate = DateTime.Now,
            ViewState = true
        });

        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<bool> RemoveDependencyAsync(int id)
    {
        var dep = await _context.TaskDependencies.FirstOrDefaultAsync(x => x.Id == id);
        if (dep == null) return false;

        _context.TaskDependencies.Remove(dep);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<DependencyRiskItemViewModel>> GetProjectRiskOverviewAsync(int projectId)
    {
        var tasks = await _context.TaskItems
            .Where(t => t.UserStory.ProjectId == projectId && t.ViewState
                && t.Status != TaskStatusType.Done && t.Status != TaskStatusType.Cancelled)
            .ToListAsync();

        var result = new List<DependencyRiskItemViewModel>();

        foreach (var task in tasks)
        {
            var delayDays = CalculateDelayDays(task);
            if (delayDays <= 0)
                continue;

            var impacted = await GetImpactedTasksAsync(task.Id, delayDays);
            var requiredImpacted = impacted.Where(x => x.IsRequiredChain).ToList();

            if (!requiredImpacted.Any())
                continue;

            result.Add(new DependencyRiskItemViewModel
            {
                TaskId = task.Id,
                Title = task.Title,
                DelayDays = delayDays,
                ImpactedTaskCount = requiredImpacted.Count,
                ImpactedTaskTitles = requiredImpacted.Select(x => x.Title).Take(5).ToList()
            });
        }

        return result
            .OrderByDescending(x => x.ImpactedTaskCount)
            .ThenByDescending(x => x.DelayDays)
            .ToList();
    }

    // Private Helpers 

    private static int CalculateDelayDays(TaskItem task)
    {
        if (task.Status == TaskStatusType.Done || task.Status == TaskStatusType.Cancelled)
            return 0;

        if (!task.DueDate.HasValue || task.DueDate.Value.Date >= DateTime.Now.Date)
            return 0;

        return (DateTime.Now.Date - task.DueDate.Value.Date).Days;
    }

    private async Task<bool> WouldCreateCycleAsync(int taskId, int dependsOnTaskId)
    {
        var visited = new HashSet<int>();
        var queue = new Queue<int>();
        queue.Enqueue(dependsOnTaskId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current == taskId)
                return true;

            if (!visited.Add(current))
                continue;

            var next = await _context.TaskDependencies
                .Where(d => d.TaskItemId == current)
                .Select(d => d.DependsOnTaskItemId)
                .ToListAsync();

            foreach (var n in next)
                queue.Enqueue(n);
        }

        return false;
    }

     public async Task<List<ImpactedTaskViewModel>> GetImpactedTasksAsync(int taskId, int delayDays)
    {
        var result = new List<ImpactedTaskViewModel>();
        var visited = new HashSet<int> { taskId };
        var queue = new Queue<(int TaskId, int Depth, bool RequiredChain)>();
        queue.Enqueue((taskId, 0, true));

        while (queue.Count > 0)
        {
            var (currentId, depth, requiredChain) = queue.Dequeue();

            var directDependents = await _context.TaskDependencies
                .Where(d => d.DependsOnTaskItemId == currentId)
                .Include(d => d.TaskItem)
                .ToListAsync();

            foreach (var dep in directDependents)
            {
                if (!visited.Add(dep.TaskItemId))
                    continue;

                var chainRequired = requiredChain && dep.IsRequired;

                result.Add(new ImpactedTaskViewModel
                {
                    TaskId = dep.TaskItemId,
                    Title = dep.TaskItem.Title,
                    Depth = depth + 1,
                    IsRequiredChain = chainRequired,
                    OriginalDueDate = dep.TaskItem.DueDate,
                    ProjectedDueDate = chainRequired && delayDays > 0 && dep.TaskItem.DueDate.HasValue
                        ? dep.TaskItem.DueDate.Value.AddDays(delayDays)
                        : dep.TaskItem.DueDate
                });

                queue.Enqueue((dep.TaskItemId, depth + 1, chainRequired));
            }
        }

        return result.OrderBy(x => x.Depth).ToList();
    }

    public async Task<List<CascadeInfoViewModel>> GetCascadeInfoAsync(int taskId)
    {
        return await _context.OverdueCascadeLogs
            .Where(x => x.ImpactedTaskId == taskId)
            .Include(x => x.SourceTask)
            .OrderByDescending(x => x.AppliedDate)
            .Select(x => new CascadeInfoViewModel
            {
                SourceTaskId = x.SourceTaskId,
                SourceTaskTitle = x.SourceTask.Title,
                DelayDaysApplied = x.DelayDaysApplied,
                AppliedDate = x.AppliedDate
            })
            .ToListAsync();
    }
}