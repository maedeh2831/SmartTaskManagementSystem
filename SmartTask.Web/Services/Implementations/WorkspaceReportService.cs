using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.ViewModels.Report;
using SmartTask.Web.Models.ViewModels.Workspace;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class WorkspaceReportService : IWorkspaceReportService
    {
        private readonly ApplicationDbContext _context;

        public WorkspaceReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<WorkspaceReportViewModel> GetReportAsync(int workspaceId, DateTime? fromDate, DateTime? toDate)
        {
            var workspace = await _context.Workspaces
                .FirstOrDefaultAsync(x => x.Id == workspaceId && x.ViewState);

            if (workspace == null)
                throw new Exception("فضای کاری یافت نشد.");

            var now = DateTime.Now;

            var tasksQuery = _context.TaskItems
                .Where(t => t.ViewState && t.UserStory.Project.WorkspaceId == workspaceId);

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
                    t.CompletedDate,
                    ProjectId = t.UserStory.ProjectId,
                    ProjectName = t.UserStory.Project.Name,
                    ProjectColor = t.UserStory.Project.Color
                })
                .ToListAsync();

            var model = new WorkspaceReportViewModel
            {
                WorkspaceId = workspace.Id,
                WorkspaceName = workspace.Name,
                FromDate = fromDate,
                ToDate = toDate,
                TotalTasks = tasks.Count,
                CompletedTasks = tasks.Count(t => t.CompletedDate.HasValue)
            };

            model.CompletionRate = model.TotalTasks == 0
                ? 0
                : Math.Round((double)model.CompletedTasks / model.TotalTasks * 100, 1);

            // ===== Task Status Chart =====
            model.TaskStatusChart = tasks
                .GroupBy(t => t.Status)
                .Select(g => new ChartPointViewModel { Label = g.Key.ToString(), Value = g.Count() })
                .ToList();

            // ===== Task Priority Chart =====
            model.TaskPriorityChart = tasks
                .GroupBy(t => t.Priority)
                .Select(g => new ChartPointViewModel { Label = g.Key.ToString(), Value = g.Count() })
                .ToList();

            // ===== Overdue Tasks =====
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
                    ProjectName = t.ProjectName,
                    DueDate = t.DueDate!.Value,
                    DaysOverdue = (now.Date - t.DueDate.Value.Date).Days
                })
                .ToList();

            // ===== Time Log By Project =====
            var timeLogsQuery = _context.TimeLogs
                .Where(x => x.ViewState && x.TaskItem.UserStory.Project.WorkspaceId == workspaceId);

            if (fromDate.HasValue)
                timeLogsQuery = timeLogsQuery.Where(x => x.CreatedDate >= fromDate.Value);
            if (toDate.HasValue)
                timeLogsQuery = timeLogsQuery.Where(x => x.CreatedDate <= toDate.Value);

            var timeLogs = await timeLogsQuery
                .Select(x => new
                {
                    x.DurationMinutes,
                    x.ApplicationUserId,
                    x.ApplicationUser.FullName,
                    x.ApplicationUser.Avatar,
                    ProjectId = x.TaskItem.UserStory.ProjectId,
                    ProjectName = x.TaskItem.UserStory.Project.Name
                })
                .ToListAsync();

            model.TimeLogByProjectChart = timeLogs
                .GroupBy(x => x.ProjectName)
                .Select(g => new ChartPointViewModel { Label = g.Key, Value = g.Sum(x => x.DurationMinutes) })
                .ToList();

            // ===== Member Workload =====
            var assignments = await _context.TaskAssignments
                .Where(a => a.ViewState && a.TaskItem.UserStory.Project.WorkspaceId == workspaceId)
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

            // ===== Project Comparison =====
            model.ProjectComparison = tasks
                .GroupBy(t => new { t.ProjectId, t.ProjectName, t.ProjectColor })
                .Select(g => new ReportProjectComparisonItemViewModel
                {
                    ProjectId = g.Key.ProjectId,
                    ProjectName = g.Key.ProjectName,
                    Color = g.Key.ProjectColor ?? "#4F46E5",
                    TotalTasks = g.Count(),
                    DoneTasks = g.Count(x => x.CompletedDate.HasValue),
                    CompletionPercentage = g.Count() == 0
                        ? 0
                        : Math.Round((double)g.Count(x => x.CompletedDate.HasValue) / g.Count() * 100, 1)
                })
                .OrderByDescending(x => x.TotalTasks)
                .ToList();

            return model;
        }
    }
}