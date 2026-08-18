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
        private readonly IDateFormatService _dateFormatService;

        private const int TopWorkspacesCount = 5;
        private const int LastNDays = 7;

        public AdminDashboardService(ApplicationDbContext context, IDateFormatService dateFormatService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dateFormatService = dateFormatService ?? throw new ArgumentNullException(nameof(dateFormatService));
        }

        public async Task<AdminDashboardViewModel> GetDashboardAsync()
        {
            var now = DateTime.Now.Date;
            var fromDate = now.AddDays(-LastNDays);

            // Execute queries sequentially — DbContext is not thread-safe
            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalWorkspaces = await _context.Workspaces.CountAsync(x => x.ViewState),
                TotalProjects = await _context.Projects.CountAsync(x => x.ViewState),
                TotalTasks = await _context.TaskItems.CountAsync(x => x.ViewState)
            };

            var userDates = await _context.Users
                .Where(x => x.CreatedDate.Date >= fromDate)
                .Select(x => x.CreatedDate.Date)
                .ToListAsync();

            var workspaceDates = await _context.Workspaces
                .Where(x => x.ViewState && x.CreateDate.Date >= fromDate)
                .Select(x => x.CreateDate.Date)
                .ToListAsync();

            model.NewUsersLast7Days = userDates.Count;
            model.NewWorkspacesLast7Days = workspaceDates.Count;

            // OPTIMIZED: Use GroupBy on client for chart data instead of O(n) loop
            var userDateGroups = userDates
                .GroupBy(d => d)
                .ToDictionary(g => g.Key, g => g.Count());

            var workspaceDateGroups = workspaceDates
                .GroupBy(d => d)
                .ToDictionary(g => g.Key, g => g.Count());

            // Build charts efficiently using pre-grouped data
            model.UserGrowthChart = BuildChartData(fromDate, now, userDateGroups, _dateFormatService);
            model.WorkspaceGrowthChart = BuildChartData(fromDate, now, workspaceDateGroups, _dateFormatService);

            // OPTIMIZED: Single query with proper projection to avoid cartesian product
            model.TopWorkspaces = await _context.Workspaces
                .Where(x => x.ViewState)
                .Select(x => new AdminTopWorkspaceItemViewModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Color = x.Color ?? "#4F46E5",
                    ProjectsCount = x.Projects.Count(p => p.ViewState),
                    MembersCount = x.Members.Count(m => m.ViewState)
                })
                .OrderByDescending(x => x.ProjectsCount)
                .Take(TopWorkspacesCount)
                .ToListAsync();

            return model;
        }

        /// <summary>
        /// OPTIMIZED: Build chart data from pre-grouped dates instead of O(n) loop per day
        /// </summary>
        private static List<ChartPointViewModel> BuildChartData(
            DateTime fromDate,
            DateTime toDate,
            Dictionary<DateTime, int> dateGroups,
            IDateFormatService dateFormatService)
        {
            var result = new List<ChartPointViewModel>();

            for (var day = fromDate; day <= toDate; day = day.AddDays(1))
            {
                result.Add(new ChartPointViewModel
                {
                    Label = dateFormatService.ToShortDisplayString(day),
                    Value = dateGroups.TryGetValue(day, out var count) ? count : 0
                });
            }

            return result;
        }
    }
}
