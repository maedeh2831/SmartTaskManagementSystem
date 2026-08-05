using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.ViewModels.Home;
using SmartTask.Web.Models.ViewModels.Workspace;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class UserDashboardService : IUserDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IActivityLogService _activityLogService;

        public UserDashboardService(ApplicationDbContext context, IActivityLogService activityLogService)
        {
            _context = context;
            _activityLogService = activityLogService;
        }

        public async Task<UserDashboardViewModel> GetDashboardAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            var now = DateTime.Now;

            var myWorkspaceIds = await _context.WorkspaceMembers
                .Where(x => x.ApplicationUserId == userId && x.ViewState)
                .Select(x => x.WorkspaceId)
                .ToListAsync();

            var model = new UserDashboardViewModel
            {
                FullName = user?.FullName ?? string.Empty,
                TotalWorkspaces = myWorkspaceIds.Count
            };

            model.TotalProjects = await _context.Projects
                .CountAsync(x => x.ViewState && myWorkspaceIds.Contains(x.WorkspaceId));

            // ===== Assigned Tasks =====
            var assignedTasks = await _context.TaskAssignments
                .Where(a => a.ViewState && a.ApplicationUserId == userId && a.TaskItem.ViewState)
                .Select(a => new
                {
                    a.TaskItem.Id,
                    a.TaskItem.Title,
                    a.TaskItem.Status,
                    a.TaskItem.DueDate,
                    a.TaskItem.CompletedDate,
                    ProjectName = a.TaskItem.UserStory.Project.Name
                })
                .ToListAsync();

            model.TotalAssignedTasks = assignedTasks.Count;
            model.CompletedAssignedTasks = assignedTasks.Count(t => t.CompletedDate.HasValue);
            model.OverdueAssignedTasks = assignedTasks.Count(t =>
                t.DueDate.HasValue && t.DueDate.Value < now && !t.CompletedDate.HasValue);

            model.TaskStatusChart = assignedTasks
                .GroupBy(t => t.Status)
                .Select(g => new ChartPointViewModel { Label = g.Key.ToString(), Value = g.Count() })
                .ToList();

            model.UpcomingTasks = assignedTasks
                .Where(t => t.DueDate.HasValue && !t.CompletedDate.HasValue
                    && t.DueDate.Value >= now && t.DueDate.Value <= now.AddDays(7))
                .OrderBy(t => t.DueDate)
                .Take(6)
                .Select(t => new UpcomingTaskItemViewModel
                {
                    Id = t.Id,
                    Title = t.Title,
                    ProjectName = t.ProjectName,
                    DueDate = t.DueDate!.Value,
                    DaysLeft = (t.DueDate.Value.Date - now.Date).Days
                })
                .ToList();

            // ===== My Workspaces =====
            model.MyWorkspaces = await _context.Workspaces
                .Where(x => x.ViewState && myWorkspaceIds.Contains(x.Id))
                .OrderByDescending(x => x.CreateDate)
                .Take(6)
                .Select(x => new DashboardWorkspaceItemViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Color = x.Color ?? "#4F46E5",
                    ProjectsCount = x.Projects.Count(p => p.ViewState),
                    MembersCount = x.Members.Count(m => m.ViewState)
                })
                .ToListAsync();

            // ===== Recent Activities (از سرویس موجود استفاده شد) =====
            var activities = await _activityLogService.GetUserActivitiesAsync(userId, 8);
            model.RecentActivities = activities.Select(x => new Models.ViewModels.Activity.ActivityItemViewModel
            {
                Id = x.Id,
                Action = x.Action,
                Description = x.Description,
                ActivityDate = x.ActivityDate,
                TaskItemId = x.TaskItemId,
                TaskTitle = x.TaskItem?.Title
            }).ToList();

            return model;
        }
    }
}