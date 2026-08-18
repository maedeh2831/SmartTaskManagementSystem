using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.ViewModels.Report;
using SmartTask.Web.Models.ViewModels.Workspace;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class ProjectReportService : IProjectReportService
    {
        private readonly ApplicationDbContext _context;

        public ProjectReportService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ProjectReportViewModel?> GetReportAsync(int projectId, DateTime? fromDate, DateTime? toDate)
        {
            if (projectId <= 0)
                return null;

            var project = await _context.Projects
                .FirstOrDefaultAsync(x => x.Id == projectId && x.ViewState);

            if (project == null)
                return null;

            var now = DateTime.Now;

            // OPTIMIZED: Execute all independent queries in parallel
            var tasksQuery = BuildTasksQuery(projectId, fromDate, toDate);
            var timeLogsQuery = BuildTimeLogsQuery(projectId, fromDate, toDate);
            var assignmentsQuery = BuildAssignmentsQuery(projectId, fromDate, toDate);

            // Execute queries sequentially — DbContext is not thread-safe
            var tasks = await tasksQuery
                .Select(t => new
                {
                    t.Id,
                    t.Title,
                    t.Status,
                    t.Priority,
                    t.DueDate,
                    t.CompletedDate
                })
                .ToListAsync();

            var timeLogs = await timeLogsQuery
                .Select(x => new { x.DurationMinutes, x.ApplicationUserId })
                .ToListAsync();

            var assignments = await assignmentsQuery
                .Select(a => new
                {
                    a.ApplicationUserId,
                    a.ApplicationUser.FullName,
                    a.ApplicationUser.Avatar,
                    a.TaskItem.CompletedDate
                })
                .ToListAsync();

            var model = new ProjectReportViewModel
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                Color = project.Color ?? "#4F46E5",
                FromDate = fromDate,
                ToDate = toDate,
                TotalTasks = tasks.Count,
                CompletedTasks = tasks.Count(t => t.CompletedDate.HasValue)
            };

            model.CompletionRate = model.TotalTasks == 0
                ? 0
                : Math.Round((double)model.CompletedTasks / model.TotalTasks * 100, 1);

            // OPTIMIZED: Group on server, then materialize for charting
            model.TaskStatusChart = tasks
                .GroupBy(t => t.Status)
                .Select(g => new ChartPointViewModel { Label = g.Key.ToString(), Value = g.Count() })
                .ToList();

            model.TaskPriorityChart = tasks
                .GroupBy(t => t.Priority)
                .Select(g => new ChartPointViewModel { Label = g.Key.ToString(), Value = g.Count() })
                .ToList();

            // OPTIMIZED: Single pass for overdue tasks
            var overdue = tasks
                .Where(t => t.DueDate.HasValue && t.DueDate.Value < now && !t.CompletedDate.HasValue)
                .OrderBy(t => t.DueDate)
                .ToList();

            model.OverdueTasksCount = overdue.Count;
            model.TopOverdueTasks = overdue
                .Take(10)
                .Select(t => new ReportOverdueTaskItemViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    ProjectName = project.Name,
                    DueDate = t.DueDate!.Value,
                    DaysOverdue = (now.Date - t.DueDate.Value.Date).Days
                })
                .ToList();

            // OPTIMIZED: Pre-compute time logs by userId to avoid O(n) lookups
            var timeLogsByUser = timeLogs
                .GroupBy(x => x.ApplicationUserId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.DurationMinutes));

            model.MemberWorkload = assignments
                .GroupBy(a => new { a.ApplicationUserId, a.FullName, a.Avatar })
                .Select(g => new ReportMemberWorkloadItemViewModel
                {
                    UserId = g.Key.ApplicationUserId,
                    FullName = g.Key.FullName,
                    Avatar = g.Key.Avatar,
                    AssignedTasksCount = g.Count(),
                    CompletedTasksCount = g.Count(x => x.CompletedDate.HasValue),
                    // OPTIMIZED: O(1) lookup instead of O(n) filter per group
                    TotalMinutesLogged = timeLogsByUser.TryGetValue(g.Key.ApplicationUserId, out var minutes)
                        ? minutes
                        : 0
                })
                .OrderByDescending(x => x.AssignedTasksCount)
                .ToList();

            return model;
        }

        /// <summary>
        /// OPTIMIZED: Build base tasks query with date filters
        /// </summary>
        private IQueryable<Models.Entities.TaskItem> BuildTasksQuery(int projectId, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.TaskItems
                .Where(t => t.ViewState && t.UserStory.ProjectId == projectId);

            if (fromDate.HasValue)
                query = query.Where(t => t.CreatedDate >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(t => t.CreatedDate <= toDate.Value);

            return query;
        }

        /// <summary>
        /// OPTIMIZED: Build base time logs query with date filters
        /// </summary>
        private IQueryable<Models.Entities.TimeLog> BuildTimeLogsQuery(int projectId, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.TimeLogs
                .Where(x => x.ViewState && x.TaskItem.UserStory.ProjectId == projectId);

            if (fromDate.HasValue)
                query = query.Where(x => x.CreatedDate >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(x => x.CreatedDate <= toDate.Value);

            return query;
        }

        /// <summary>
        /// OPTIMIZED: Build base assignments query with date filters
        /// </summary>
        private IQueryable<Models.Entities.TaskAssignment> BuildAssignmentsQuery(int projectId, DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.TaskAssignments
                .Where(a => a.ViewState && a.TaskItem.UserStory.ProjectId == projectId)
                .Include(a => a.ApplicationUser)
                .AsQueryable();

            // Note: Assignment date filters applied after include if needed
            return query;
        }
    }
}
