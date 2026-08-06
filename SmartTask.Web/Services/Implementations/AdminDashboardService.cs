using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.ViewModels.Admin;
using SmartTask.Web.Models.ViewModels.Workspace;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly ApplicationDbContext _context;

        public AdminDashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardViewModel> GetDashboardAsync()
        {
            var now = DateTime.Now.Date;
            var fromDate = now.AddDays(-6);

            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalWorkspaces = await _context.Workspaces.CountAsync(x => x.ViewState),
                TotalProjects = await _context.Projects.CountAsync(x => x.ViewState),
                TotalTasks = await _context.TaskItems.CountAsync(x => x.ViewState)
            };

            var userDates = await _context.Users
                .Select(x => x.CreatedDate)
                .ToListAsync();

            var workspaceDates = await _context.Workspaces
                .Where(x => x.ViewState)
                .Select(x => x.CreateDate)
                .ToListAsync();

            model.NewUsersLast7Days = userDates.Count(d => d.Date >= fromDate);
            model.NewWorkspacesLast7Days = workspaceDates.Count(d => d.Date >= fromDate);

            var userChart = new List<ChartPointViewModel>();
            var workspaceChart = new List<ChartPointViewModel>();

            for (var day = fromDate; day <= now; day = day.AddDays(1))
            {
                userChart.Add(new ChartPointViewModel
                {
                    Label = day.ToString("MM/dd"),
                    Value = userDates.Count(d => d.Date == day)
                });

                workspaceChart.Add(new ChartPointViewModel
                {
                    Label = day.ToString("MM/dd"),
                    Value = workspaceDates.Count(d => d.Date == day)
                });
            }

            model.UserGrowthChart = userChart;
            model.WorkspaceGrowthChart = workspaceChart;

            model.TopWorkspaces = await _context.Workspaces
                .Where(x => x.ViewState)
                .OrderByDescending(x => x.Projects.Count(p => p.ViewState))
                .Take(5)
                .Select(x => new AdminTopWorkspaceItemViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Color = x.Color ?? "#4F46E5",
                    ProjectsCount = x.Projects.Count(p => p.ViewState),
                    MembersCount = x.Members.Count(m => m.ViewState)
                })
                .ToListAsync();

            return model;
        }
    }
}