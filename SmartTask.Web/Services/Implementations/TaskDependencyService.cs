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
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
    }

    public async Task<TaskDependencyWidgetViewModel> GetWidgetAsync(int taskId, int currentUserId)
    {
        if (taskId <= 0 || currentUserId <= 0)
            throw new ArgumentException("Invalid task or user ID");

        var task = await _context.TaskItems
            .Include(t => t.UserStory)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            throw new InvalidOperationException("Task یافت نشد.");

        var projectId = task.UserStory.ProjectId;

        // OPTIMIZED: Load all dependencies in 2 queries instead of separate queries
        var dependencies = await _context.TaskDependencies
            .Where(d => (d.TaskItemId == taskId || d.DependsOnTaskItemId == taskId) && d.ViewState)
            .Include(d => d.TaskItem)
            .Include(d => d.DependsOnTaskItem)
            .ToListAsync();

        var dependsOnRaw = dependencies.Where(d => d.TaskItemId == taskId).ToList();
        var dependentsRaw = dependencies.Where(d => d.DependsOnTaskItemId == taskId).ToList();

        var linkedIds = new HashSet<int>(dependsOnRaw.Select(d => d.DependsOnTaskItemId));
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
        if (taskId <= 0 || dependsOnTaskId <= 0)
            return (false, "معرفات غیر معتبر.");

        if (taskId == dependsOnTaskId)
            return (false, "یک Task نمی‌تواند به خودش وابسته باشد.");

        var exists = await _context.TaskDependencies
            .AnyAsync(d => d.TaskItemId == taskId && d.DependsOnTaskItemId == dependsOnTaskId && d.ViewState);

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
        if (id <= 0)
            return false;

        var dep = await _context.TaskDependencies.FirstOrDefaultAsync(x => x.Id == id);
        if (dep == null) return false;

        dep.ViewState = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<DependencyRiskItemViewModel>> GetProjectRiskOverviewAsync(int projectId)
    {
        if (projectId <= 0)
            return new List<DependencyRiskItemViewModel>();

        // OPTIMIZED: Load all tasks and dependencies upfront (2 queries total)
        var tasks = await _context.TaskItems
            .Where(t => t.UserStory.ProjectId == projectId && t.ViewState
                && t.Status != TaskStatusType.Done && t.Status != TaskStatusType.Cancelled)
            .Select(t => new { t.Id, t.Title, t.DueDate, t.Status })
            .ToListAsync();

        if (tasks.Count == 0)
            return new List<DependencyRiskItemViewModel>();

        var taskIds = tasks.Select(t => t.Id).ToHashSet();

        // Load ALL dependencies for the entire graph in ONE query
        var allDependencies = await _context.TaskDependencies
            .Where(d => taskIds.Contains(d.DependsOnTaskItemId) || taskIds.Contains(d.TaskItemId))
            .Where(d => d.ViewState)
            .ToListAsync();

        var result = new List<DependencyRiskItemViewModel>();

        foreach (var task in tasks)
        {
            var delayDays = CalculateDelayDays(new TaskItem
            {
                Id = task.Id,
                Title = task.Title,
                DueDate = task.DueDate,
                Status = task.Status,
                ViewState = true
            });

            if (delayDays <= 0)
                continue;

            // OPTIMIZED: Traverse dependency graph in-memory instead of DB queries
            var impacted = GetImpactedTasksInMemory(task.Id, delayDays, allDependencies, taskIds);
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

    public async Task<DependencyGraphViewModel> GetDependencyGraphAsync(int projectId)
    {
        if (projectId <= 0)
            return new DependencyGraphViewModel { Nodes = new(), Edges = new() };

        // OPTIMIZED: Single query with proper projections
        var tasks = await _context.TaskItems
            .Where(t => t.UserStory.ProjectId == projectId && t.ViewState)
            .Select(t => new { t.Id, t.Title, t.Status, t.DueDate })
            .ToListAsync();

        if (tasks.Count == 0)
            return new DependencyGraphViewModel { Nodes = new(), Edges = new() };

        var taskIds = tasks.Select(t => t.Id).ToHashSet();

        var dependencies = await _context.TaskDependencies
            .Where(d => taskIds.Contains(d.TaskItemId) && taskIds.Contains(d.DependsOnTaskItemId) && d.ViewState)
            .ToListAsync();

        var riskyTaskIds = (await GetProjectRiskOverviewAsync(projectId))
            .Select(r => r.TaskId)
            .ToHashSet();

        var now = DateTime.Now.Date;

        var nodes = tasks.Select(t => new DependencyGraphNodeViewModel
        {
            Id = t.Id,
            Title = t.Title,
            IsDone = t.Status == TaskStatusType.Done || t.Status == TaskStatusType.Cancelled,
            IsOverdue = t.DueDate.HasValue && t.DueDate.Value.Date < now
                && t.Status != TaskStatusType.Done && t.Status != TaskStatusType.Cancelled,
            IsAtRisk = riskyTaskIds.Contains(t.Id)
        }).ToList();

        var edges = dependencies.Select(d => new DependencyGraphEdgeViewModel
        {
            SourceTaskId = d.DependsOnTaskItemId,
            TargetTaskId = d.TaskItemId,
            IsRequired = d.IsRequired
        }).ToList();

        return new DependencyGraphViewModel { Nodes = nodes, Edges = edges };
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
        if (taskId <= 0 || dependsOnTaskId <= 0)
            return false;

        // OPTIMIZED: Load all dependencies once, then traverse in-memory
        var allDependencies = await _context.TaskDependencies
            .Where(d => d.ViewState)
            .Select(d => new { d.TaskItemId, d.DependsOnTaskItemId })
            .ToListAsync();

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

            // Traverse in-memory dependencies
            var next = allDependencies
                .Where(d => d.TaskItemId == current)
                .Select(d => d.DependsOnTaskItemId)
                .ToList();

            foreach (var n in next)
                queue.Enqueue(n);
        }

        return false;
    }

    public async Task<List<ImpactedTaskViewModel>> GetImpactedTasksAsync(int taskId, int delayDays)
    {
        if (taskId <= 0)
            return new List<ImpactedTaskViewModel>();

        // OPTIMIZED: Load all dependencies for the entire graph in ONE query
        var allDependencies = await _context.TaskDependencies
            .Where(d => d.ViewState)
            .Select(d => new
            {
                d.Id,
                d.TaskItemId,
                d.DependsOnTaskItemId,
                d.IsRequired,
                TaskTitle = d.TaskItem.Title,
                TaskDueDate = d.TaskItem.DueDate
            })
            .ToListAsync();

        // Traverse in-memory instead of making DB queries per node
        var result = new List<ImpactedTaskViewModel>();
        var visited = new HashSet<int> { taskId };
        var queue = new Queue<(int TaskId, int Depth, bool RequiredChain)>();
        queue.Enqueue((taskId, 0, true));

        while (queue.Count > 0)
        {
            var (currentId, depth, requiredChain) = queue.Dequeue();

            // Get direct dependents from in-memory collection
            var directDependents = allDependencies
                .Where(d => d.DependsOnTaskItemId == currentId)
                .ToList();

            foreach (var dep in directDependents)
            {
                if (!visited.Add(dep.TaskItemId))
                    continue;

                var chainRequired = requiredChain && dep.IsRequired;

                result.Add(new ImpactedTaskViewModel
                {
                    TaskId = dep.TaskItemId,
                    Title = dep.TaskTitle,
                    Depth = depth + 1,
                    IsRequiredChain = chainRequired,
                    OriginalDueDate = dep.TaskDueDate,
                    ProjectedDueDate = chainRequired && delayDays > 0 && dep.TaskDueDate.HasValue
                        ? dep.TaskDueDate.Value.AddDays(delayDays)
                        : dep.TaskDueDate
                });

                queue.Enqueue((dep.TaskItemId, depth + 1, chainRequired));
            }
        }

        return result.OrderBy(x => x.Depth).ToList();
    }

    /// <summary>
    /// OPTIMIZED: In-memory traversal of dependency graph to avoid N+1 queries
    /// </summary>
    private List<ImpactedTaskViewModel> GetImpactedTasksInMemory(int taskId, int delayDays,
        List<TaskDependency> allDependencies, HashSet<int> validTaskIds)
    {
        var result = new List<ImpactedTaskViewModel>();
        var visited = new HashSet<int> { taskId };
        var queue = new Queue<(int TaskId, int Depth, bool RequiredChain)>();
        queue.Enqueue((taskId, 0, true));

        while (queue.Count > 0)
        {
            var (currentId, depth, requiredChain) = queue.Dequeue();

            var directDependents = allDependencies
                .Where(d => d.DependsOnTaskItemId == currentId && d.ViewState)
                .ToList();

            foreach (var dep in directDependents)
            {
                if (!visited.Add(dep.TaskItemId) || !validTaskIds.Contains(dep.TaskItemId))
                    continue;

                var chainRequired = requiredChain && dep.IsRequired;

                result.Add(new ImpactedTaskViewModel
                {
                    TaskId = dep.TaskItemId,
                    Title = dep.TaskItem?.Title ?? "Unknown",
                    Depth = depth + 1,
                    IsRequiredChain = chainRequired,
                    OriginalDueDate = dep.TaskItem?.DueDate,
                    ProjectedDueDate = chainRequired && delayDays > 0 && dep.TaskItem?.DueDate.HasValue == true
                        ? dep.TaskItem.DueDate.Value.AddDays(delayDays)
                        : dep.TaskItem?.DueDate
                });

                queue.Enqueue((dep.TaskItemId, depth + 1, chainRequired));
            }
        }

        return result.OrderBy(x => x.Depth).ToList();
    }

    public async Task<List<CascadeInfoViewModel>> GetCascadeInfoAsync(int taskId)
    {
        if (taskId <= 0)
            return new List<CascadeInfoViewModel>();

        return await _context.OverdueCascadeLogs
            .Where(x => x.ImpactedTaskId == taskId && x.ViewState)
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
