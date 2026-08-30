using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.ViewModels.ProjectSimulation;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class ImpactAnalysisService : IImpactAnalysisService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICriticalPathAnalyzer _criticalPathAnalyzer;
        private readonly ILogger<ImpactAnalysisService> _logger;

        public ImpactAnalysisService(
            ApplicationDbContext context,
            ICriticalPathAnalyzer criticalPathAnalyzer,
            ILogger<ImpactAnalysisService> logger)
        {
            _context = context;
            _criticalPathAnalyzer = criticalPathAnalyzer;
            _logger = logger;
        }

        public async Task<ImpactAnalysisDto> AnalyzeImpactAsync(int projectId, int taskId, int delayDays)
        {
            var delayedTask = await _context.TaskItems
                .FirstOrDefaultAsync(t => t.Id == taskId && t.ViewState);

            if (delayedTask == null)
                throw new ArgumentException($"Task {taskId} not found");

            var project = await _context.Projects
                .Include(p => p.UserStories)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                throw new ArgumentException($"Project {projectId} not found");

            // Get critical path information
            var criticalPath = await _criticalPathAnalyzer.CalculateCriticalPathAsync(projectId);
            var originalProjectEndDate = criticalPath.ProjectEndDate;

            // Find all downstream tasks
            var downstreamTasks = await GetDownstreamTasksAsync(taskId);
            downstreamTasks.Add(taskId); // Include the delayed task itself

            // Calculate new end dates
            var newEndDates = await CalculateNewEndDatesAsync(downstreamTasks, delayDays);

            // Build affected tasks list with dependency paths
            var affectedTasks = new List<AffectedTaskDto>();
            foreach (var affectedTaskId in downstreamTasks.OrderBy(t => t))
            {
                var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == affectedTaskId);
                if (task == null) continue;

                var oldEndDate = task.DueDate ?? DateTime.Now.AddDays(task.Estimate);
                var newEndDate = newEndDates[affectedTaskId];
                var daysShifted = (int)(newEndDate - oldEndDate).TotalDays;

                var dependencyPath = await BuildDependencyPathAsync(taskId, affectedTaskId);

                affectedTasks.Add(new AffectedTaskDto
                {
                    TaskId = affectedTaskId,
                    TaskTitle = task.Title,
                    OriginalEndDate = oldEndDate,
                    NewEndDate = newEndDate,
                    DaysShifted = daysShifted,
                    DependencyPath = dependencyPath,
                    DepthInDependencyChain = await GetDependencyDepthAsync(taskId, affectedTaskId)
                });
            }

            // Determine new project end date (max of all affected tasks)
            var newProjectEndDate = affectedTasks.Any()
                ? affectedTasks.Max(t => t.NewEndDate)
                : originalProjectEndDate.AddDays(delayDays);

            var projectDelayDays = (int)(newProjectEndDate - originalProjectEndDate).TotalDays;

            // Calculate ripple effects
            var rippleEffects = await CalculateRippleEffectsAsync(downstreamTasks);

            var riskLevel = CalculateRiskLevel(affectedTasks.Count, delayDays, criticalPath.CriticalPathLengthDays);

            _logger.LogInformation(
                $"Impact analysis for project {projectId}, task {taskId}, delay {delayDays}d: " +
                $"{affectedTasks.Count} tasks affected, {projectDelayDays}d project delay, {riskLevel} risk");

            return new ImpactAnalysisDto
            {
                DelayedTaskId = taskId,
                DelayedTaskTitle = delayedTask.Title,
                DelayDays = delayDays,
                OriginalProjectEndDate = originalProjectEndDate,
                NewProjectEndDate = newProjectEndDate,
                ProjectDelayDays = projectDelayDays,
                TotalAffectedTasks = affectedTasks.Count,
                RiskLevel = riskLevel,
                AffectedTasks = affectedTasks,
                RippleEffects = rippleEffects,
                AnalysisGeneratedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Uses DFS to find all downstream tasks that depend on the given task (directly or indirectly)
        /// </summary>
        public async Task<List<int>> GetDownstreamTasksAsync(int taskId)
        {
            var downstreamTasks = new List<int>();
            var visited = new HashSet<int>();
            var stack = new Stack<int>();

            stack.Push(taskId);

            while (stack.Count > 0)
            {
                var currentTaskId = stack.Pop();
                if (visited.Contains(currentTaskId)) continue;
                visited.Add(currentTaskId);

                // Find tasks that depend on current task
                var dependentTasks = await _context.TaskDependencies
                    .Where(d => d.DependsOnTaskItemId == currentTaskId)
                    .Select(d => d.TaskItemId)
                    .ToListAsync();

                foreach (var depTaskId in dependentTasks)
                {
                    if (!visited.Contains(depTaskId))
                    {
                        downstreamTasks.Add(depTaskId);
                        stack.Push(depTaskId);
                    }
                }
            }

            return downstreamTasks;
        }

        /// <summary>
        /// Calculates new end dates for affected tasks based on delay propagation through dependency chain
        /// </summary>
        public async Task<Dictionary<int, DateTime>> CalculateNewEndDatesAsync(List<int> affectedTaskIds, int delayDays)
        {
            var newEndDates = new Dictionary<int, DateTime>();

            foreach (var taskId in affectedTaskIds)
            {
                var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId);
                if (task == null) continue;

                var baseEndDate = task.DueDate ?? DateTime.Now.AddDays(task.Estimate);
                var newEndDate = baseEndDate.AddDays(delayDays);
                newEndDates[taskId] = newEndDate;
            }

            return newEndDates;
        }

        /// <summary>
        /// Determines risk level based on:
        /// - Number of affected tasks
        /// - Delay duration relative to critical path length
        /// - Impact percentage
        /// </summary>
        public string CalculateRiskLevel(int affectedTasksCount, int delayDays, int projectCriticalPathLength)
        {
            if (delayDays <= 0)
                return "Low";

            var delayPercentage = projectCriticalPathLength > 0
                ? (double)delayDays / projectCriticalPathLength * 100
                : 0;

            // Risk matrix
            if (delayPercentage >= 20 || affectedTasksCount >= 10)
                return "High";
            else if (delayPercentage >= 10 || affectedTasksCount >= 5)
                return "Medium";
            else
                return "Low";
        }

        private async Task<string> BuildDependencyPathAsync(int sourceTaskId, int targetTaskId)
        {
            if (sourceTaskId == targetTaskId)
                return "Direct";

            var path = new List<int> { sourceTaskId };
            var visited = new HashSet<int>();
            var queue = new Queue<int>();
            queue.Enqueue(sourceTaskId);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (visited.Contains(current)) continue;
                visited.Add(current);

                var dependents = await _context.TaskDependencies
                    .Where(d => d.DependsOnTaskItemId == current)
                    .Select(d => d.TaskItemId)
                    .ToListAsync();

                foreach (var dep in dependents)
                {
                    path.Add(dep);
                    if (dep == targetTaskId)
                        return string.Join(" -> ", path);
                    queue.Enqueue(dep);
                }
            }

            return "Indirect";
        }

        private async Task<int> GetDependencyDepthAsync(int sourceTaskId, int targetTaskId)
        {
            if (sourceTaskId == targetTaskId)
                return 0;

            var depth = 0;
            var visited = new HashSet<int>();
            var queue = new Queue<(int taskId, int level)>();
            queue.Enqueue((sourceTaskId, 0));

            while (queue.Count > 0)
            {
                var (current, level) = queue.Dequeue();
                if (visited.Contains(current)) continue;
                visited.Add(current);

                var dependents = await _context.TaskDependencies
                    .Where(d => d.DependsOnTaskItemId == current)
                    .Select(d => d.TaskItemId)
                    .ToListAsync();

                foreach (var dep in dependents)
                {
                    if (dep == targetTaskId)
                        return level + 1;
                    queue.Enqueue((dep, level + 1));
                }
            }

            return 0;
        }

        private async Task<List<RippleEffectDto>> CalculateRippleEffectsAsync(List<int> affectedTaskIds)
        {
            var rippleEffects = new List<RippleEffectDto>();

            foreach (var taskId in affectedTaskIds.Take(5)) // Top 5 to avoid too much data
            {
                var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId);
                if (task == null) continue;

                var directDependencies = await _context.TaskDependencies
                    .CountAsync(d => d.DependsOnTaskItemId == taskId);

                var indirectDependencies = await GetDownstreamTasksAsync(taskId);

                rippleEffects.Add(new RippleEffectDto
                {
                    TaskId = taskId,
                    TaskTitle = task.Title,
                    DirectDependenciesAffected = directDependencies,
                    IndirectDependenciesAffected = indirectDependencies.Count,
                    TotalDownstreamTasks = directDependencies + indirectDependencies.Count,
                    SeverityLevel = (directDependencies + indirectDependencies.Count) switch
                    {
                        >= 10 => "High",
                        >= 5 => "Medium",
                        _ => "Low"
                    }
                });
            }

            return rippleEffects;
        }
    }
}
