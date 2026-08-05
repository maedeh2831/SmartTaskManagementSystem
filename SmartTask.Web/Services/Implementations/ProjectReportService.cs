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
            _context = context;
        }

        public async Task<ProjectReportViewModel?> GetReportAsync(int projectId, DateTime? fromDate, DateTime? toDate)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(x => x.Id == projectId && x.ViewState);

            if (project == null)
                return null;

            var now = DateTime.Now;

            var tasksQuery = _context.TaskItems
                .Where(t => t.ViewState && t.UserStory.ProjectId == projectId);

            if (fromDate.HasValue)
                tasksQuery = tasksQuery.Where(t => t.CreatedDate >= fromDate.Value);
            if (toDate.HasValue)
                tasksQuery = tasksQuery.Where(t => t.CreatedDate <= toDate.Value);

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

            model.TaskStatusChart = tasks
                .GroupBy(t => t.Status)
                .Select(g => new ChartPointViewModel { Label = g.Key.ToString(), Value = g.Count() })
                .ToList();

            model.TaskPriorityChart = tasks
                .GroupBy(t => t.Priority)
                .Select(g => new ChartPointViewModel { Label = g.Key.ToString(), Value = g.Count() })
                .ToList();

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

            var timeLogsQuery = _context.TimeLogs
                .Where(x => x.ViewState && x.TaskItem.UserStory.ProjectId == projectId);

            if (fromDate.HasValue)
                timeLogsQuery = timeLogsQuery.Where(x => x.CreatedDate >= fromDate.Value);
            if (toDate.HasValue)
                timeLogsQuery = timeLogsQuery.Where(x => x.CreatedDate <= toDate.Value);

            var timeLogs = await timeLogsQuery
                .Select(x => new { x.DurationMinutes, x.ApplicationUserId })
                .ToListAsync();

            var assignments = await _context.TaskAssignments
                .Where(a => a.ViewState && a.TaskItem.UserStory.ProjectId == projectId)
                .Select(a => new
                {
                    a.ApplicationUserId,
                    a.ApplicationUser.FullName,
                    a.ApplicationUser.Avatar,
                    a.TaskItem.CompletedDate
                })
                .ToListAsync();

            model.MemberWorkload = assignments
                .GroupBy(a => new { a.ApplicationUserId, a.FullName, a.Avatar })
                .Select(g => new ReportMemberWorkloadItemViewModel
                {
                    UserId = g.Key.ApplicationUserId,
                    FullName = g.Key.FullName,
                    Avatar = g.Key.Avatar,
                    AssignedTasksCount = g.Count(),
                    CompletedTasksCount = g.Count(x => x.CompletedDate.HasValue),
                    TotalMinutesLogged = timeLogs
                        .Where(x => x.ApplicationUserId == g.Key.ApplicationUserId)
                        .Sum(x => x.DurationMinutes)
                })
                .OrderByDescending(x => x.AssignedTasksCount)
                .ToList();

            return model;
        }
    }
}