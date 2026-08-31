using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.ViewModels.ProjectSimulation;
using SmartTask.Web.Services.Interfaces;
using System.Collections.Generic;
using System.Diagnostics;

namespace SmartTask.Web.Services.Implementations
{
    public class CriticalPathAnalyzer : ICriticalPathAnalyzer
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CriticalPathAnalyzer> _logger;

        public CriticalPathAnalyzer(ApplicationDbContext context, ILogger<CriticalPathAnalyzer> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Implements Critical Path Method (CPM) using topological sort and dynamic programming.
        /// Algorithm:
        /// 1. Build dependency graph from TaskDependency records
        /// 2. Calculate earliest start/finish times (forward pass)
        /// 3. Calculate latest start/finish times (backward pass)
        /// 4. Identify critical path (tasks with zero slack)
        /// </summary>
        public async Task<CriticalPathDto> CalculateCriticalPathAsync(int projectId)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                var project = await _context.Projects
                    .Include(p => p.UserStories)
                    .FirstOrDefaultAsync(p => p.Id == projectId);

                if (project == null)
                    throw new ArgumentException($"Project {projectId} not found");

                // Get all tasks in project with their dependencies
                var userStoryIds = project.UserStories.Select(us => us.Id).ToList();

                var allTasks = await _context.TaskItems
                    .Where(t => userStoryIds.Contains(t.UserStoryId) && t.ViewState)
                    .Include(t => t.UserStory)
                    .ToListAsync();

                if (!allTasks.Any())
                {
                    return new CriticalPathDto
                    {
                        CriticalPathTaskIds = new List<int>(),
                        CriticalPathLengthDays = 0,
                        TaskSlackTimes = new List<TaskSlackDto>(),
                        ProjectStartDate = project.StartDate ?? DateTime.Now,
                        ProjectEndDate = project.DueDate ?? DateTime.Now,
                        TotalTasksInPath = 0
                    };
                }

                var taskIds = allTasks.Select(t => t.Id).ToList();

                // فقط وابستگی‌هایی که هر دو سرشان داخل همین پروژه است؛
                // در غیر این صورت دسترسی به graph[...] استثنا می‌دهد.
                var dependencies = await _context.TaskDependencies
                    .Where(d => taskIds.Contains(d.TaskItemId) &&
                                taskIds.Contains(d.DependsOnTaskItemId))
                    .ToListAsync();

                // Build adjacency list for graph
                var graph = new Dictionary<int, List<int>>();
                var reverseGraph = new Dictionary<int, List<int>>();
                var inDegree = new Dictionary<int, int>();
                var outDegree = new Dictionary<int, int>();

                foreach (var task in allTasks)
                {
                    graph[task.Id] = new List<int>();
                    reverseGraph[task.Id] = new List<int>();
                    inDegree[task.Id] = 0;
                    outDegree[task.Id] = 0;
                }

                foreach (var dep in dependencies)
                {
                    graph[dep.DependsOnTaskItemId].Add(dep.TaskItemId);
                    reverseGraph[dep.TaskItemId].Add(dep.DependsOnTaskItemId);
                    inDegree[dep.TaskItemId]++;
                    outDegree[dep.DependsOnTaskItemId]++;
                }

                // Find root tasks (no dependencies)
                var rootTasks = allTasks.Where(t => inDegree[t.Id] == 0).Select(t => t.Id).ToList();

                // Forward pass: Calculate earliest start/finish times
                var earliestStart = new Dictionary<int, DateTime>();
                var earliestFinish = new Dictionary<int, DateTime>();
                var projectStart = project.StartDate ?? DateTime.Now;

                foreach (var task in allTasks)
                {
                    earliestStart[task.Id] = projectStart;
                    earliestFinish[task.Id] = projectStart;
                }

                var queue = new Queue<int>(rootTasks);
                var visited = new HashSet<int>();

                while (queue.Count > 0)
                {
                    var taskId = queue.Dequeue();
                    if (visited.Contains(taskId)) continue;
                    visited.Add(taskId);

                    var task = allTasks.First(t => t.Id == taskId);
                    var estimate = task.Estimate > 0 ? task.Estimate : 1;

                    // Calculate earliest finish
                    if (inDegree[taskId] == 0)
                    {
                        earliestStart[taskId] = task.StartDate ?? projectStart;
                    }
                    else
                    {
                        // Earliest start = max earliest finish of predecessors
                        var predecessorIds = reverseGraph[taskId];
                        if (predecessorIds.Any(p => !visited.Contains(p)))
                            continue; // Wait for all predecessors to be processed

                        earliestStart[taskId] = predecessorIds.Any()
                            ? predecessorIds.Max(p => earliestFinish[p])
                            : projectStart;
                    }

                    earliestFinish[taskId] = earliestStart[taskId].AddDays(estimate);

                    // Add successors to queue
                    foreach (var successor in graph[taskId])
                    {
                        queue.Enqueue(successor);
                    }
                }

                var projectEnd = earliestFinish.Values.Max();

                // Backward pass: Calculate latest start/finish times
                var leafTasks = allTasks.Where(t => outDegree[t.Id] == 0).Select(t => t.Id).ToList();
                var latestFinish = new Dictionary<int, DateTime>();
                var latestStart = new Dictionary<int, DateTime>();

                foreach (var task in allTasks)
                {
                    latestFinish[task.Id] = projectEnd;
                    latestStart[task.Id] = projectEnd;
                }

                var reverseQueue = new Queue<int>(leafTasks);
                var reverseVisited = new HashSet<int>();

                while (reverseQueue.Count > 0)
                {
                    var taskId = reverseQueue.Dequeue();
                    if (reverseVisited.Contains(taskId)) continue;
                    reverseVisited.Add(taskId);

                    var task = allTasks.First(t => t.Id == taskId);
                    var estimate = task.Estimate > 0 ? task.Estimate : 1;

                    if (outDegree[taskId] == 0)
                    {
                        latestFinish[taskId] = earliestFinish[taskId];
                    }
                    else
                    {
                        var successorIds = graph[taskId];
                        if (successorIds.Any(s => !reverseVisited.Contains(s)))
                            continue;

                        latestFinish[taskId] = successorIds.Any()
                            ? successorIds.Min(s => latestStart[s])
                            : projectEnd;
                    }

                    latestStart[taskId] = latestFinish[taskId].AddDays(-estimate);

                    foreach (var predecessor in reverseGraph[taskId])
                    {
                        reverseQueue.Enqueue(predecessor);
                    }
                }

                // Calculate slack times and identify critical path
                var taskSlacks = new List<TaskSlackDto>();
                var criticalPathTasks = new List<int>();
                const double SLACK_TOLERANCE = 0.1; // Allow small floating point differences

                foreach (var task in allTasks)
                {
                    var slackTime = (int)(latestStart[task.Id] - earliestStart[task.Id]).TotalDays;
                    var isOnCriticalPath = slackTime <= SLACK_TOLERANCE;

                    if (isOnCriticalPath)
                        criticalPathTasks.Add(task.Id);

                    taskSlacks.Add(new TaskSlackDto
                    {
                        TaskId = task.Id,
                        TaskTitle = task.Title,
                        SlackTimeDays = Math.Max(0, slackTime),
                        IsOnCriticalPath = isOnCriticalPath,
                        StartDate = earliestStart[task.Id],
                        EndDate = earliestFinish[task.Id],
                        EstimateDays = task.Estimate > 0 ? task.Estimate : 1
                    });
                }

                var criticalPathLength = (int)(projectEnd - projectStart).TotalDays;

                sw.Stop();
                _logger.LogInformation($"Critical path calculation completed in {sw.ElapsedMilliseconds}ms for project {projectId}");

                return new CriticalPathDto
                {
                    CriticalPathTaskIds = criticalPathTasks,
                    CriticalPathLengthDays = criticalPathLength,
                    TaskSlackTimes = taskSlacks.OrderBy(t => t.IsOnCriticalPath ? 0 : 1).ToList(),
                    ProjectStartDate = projectStart,
                    ProjectEndDate = projectEnd,
                    TotalTasksInPath = criticalPathTasks.Count
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating critical path for project {projectId}");
                throw;
            }
        }

        public async Task<List<int>> GetCriticalPathTasksAsync(int projectId)
        {
            var criticalPath = await CalculateCriticalPathAsync(projectId);
            return criticalPath.CriticalPathTaskIds;
        }

        public async Task<int> GetTaskSlackTimeAsync(int taskId)
        {
            var task = await _context.TaskItems
                .Include(t => t.UserStory)
                .FirstOrDefaultAsync(t => t.Id == taskId);

            if (task == null)
                throw new ArgumentException($"Task {taskId} not found");

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.UserStories.Any(us => us.Id == task.UserStoryId));

            if (project == null)
                throw new ArgumentException($"Project for task {taskId} not found");

            var criticalPath = await CalculateCriticalPathAsync(project.Id);
            var slackInfo = criticalPath.TaskSlackTimes.FirstOrDefault(t => t.TaskId == taskId);

            return slackInfo?.SlackTimeDays ?? 0;
        }
    }
}
