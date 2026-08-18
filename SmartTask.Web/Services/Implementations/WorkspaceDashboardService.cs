using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Workspace;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations;

public class WorkspaceDashboardService : IWorkspaceDashboardService
{
    private readonly ApplicationDbContext _context;
    private const int RecentItemsCount = 5;
    private const int TopMembersCount = 6;
    private const int ActivityItemsCount = 8;
    private const int DaysLookback = 6;

    public WorkspaceDashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WorkspaceDashboardViewModel> GetDashboardAsync(int workspaceId, int currentUserId)
    {
        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(x => x.Id == workspaceId && x.ViewState)
            ?? throw new InvalidOperationException("فضای کاری یافت نشد.");

        var model = new WorkspaceDashboardViewModel
        {
            WorkspaceId = workspace.Id,
            WorkspaceName = workspace.Name,
            WorkspaceColor = workspace.Color ?? "#4F46E5"
        };

        // Execute queries sequentially — DbContext is not thread-safe
        var stats = await GetStatisticsAsync(workspaceId);
        model.TotalMembers = stats.TotalMembers;
        model.TotalProjects = stats.TotalProjects;
        model.ActiveProjects = stats.ActiveProjects;
        model.PendingInvitations = stats.PendingInvitations;

        model.RecentProjects = await GetRecentProjectsAsync(workspaceId);
        model.TopMembers = await GetTopMembersAsync(workspaceId, currentUserId);
        model.RecentActivities = await GetRecentActivitiesAsync(workspaceId);

        var charts = await GetChartsDataAsync(workspaceId);
        model.ProjectStatusChart = charts.StatusChart;
        model.ActivityChart = charts.ActivityChart;

        return model;
    }

    private async Task<(int TotalMembers, int TotalProjects, int ActiveProjects, int PendingInvitations)> GetStatisticsAsync(int workspaceId)
    {
        var totalMembers = await _context.WorkspaceMembers
            .CountAsync(x => x.WorkspaceId == workspaceId && x.ViewState);

        var totalProjects = await _context.Projects
            .CountAsync(x => x.WorkspaceId == workspaceId && x.ViewState);

        var activeProjects = await _context.Projects
            .CountAsync(x => x.WorkspaceId == workspaceId && x.ViewState && x.Status == ProjectStatusType.Active);

        var pendingInvitations = await _context.WorkspaceInvitations
            .CountAsync(x => x.WorkspaceId == workspaceId
                && x.Status == WorkspaceInvitationStatusType.Pending
                && x.ExpiryDate >= DateTime.Now);

        return (totalMembers, totalProjects, activeProjects, pendingInvitations);
    }

    private async Task<List<DashboardProjectItemViewModel>> GetRecentProjectsAsync(int workspaceId)
    {
        return await _context.Projects
            .Where(x => x.WorkspaceId == workspaceId && x.ViewState)
            .OrderByDescending(x => x.CreatedDate)
            .Take(RecentItemsCount)
            .Select(x => new DashboardProjectItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Color = x.Color ?? "#4F46E5",
                Status = x.Status,
                CreateDate = x.CreatedDate
            })
            .ToListAsync();
    }

    private async Task<List<WorkspaceMemberViewModel>> GetTopMembersAsync(int workspaceId, int currentUserId)
    {
        return await _context.WorkspaceMembers
            .Where(x => x.WorkspaceId == workspaceId && x.ViewState)
            .Include(x => x.ApplicationUser)
            .OrderBy(x => x.Role)
            .Take(TopMembersCount)
            .Select(x => new WorkspaceMemberViewModel
            {
                Id = x.Id,
                WorkspaceId = x.WorkspaceId,
                UserId = x.ApplicationUserId,
                FullName = x.ApplicationUser.FullName,
                Email = x.ApplicationUser.Email!,
                Avatar = x.ApplicationUser.Avatar,
                Role = x.Role,
                IsOwner = x.Role == WorkspaceRoleType.Owner,
                IsCurrentUser = x.ApplicationUserId == currentUserId
            })
            .ToListAsync();
    }

    private async Task<List<DashboardActivityItemViewModel>> GetRecentActivitiesAsync(int workspaceId)
    {
        var memberActivities = await _context.WorkspaceMembers
            .Where(x => x.WorkspaceId == workspaceId && x.ViewState)
            .OrderByDescending(x => x.CreatedDate)
            .Take(RecentItemsCount)
            .Select(x => new DashboardActivityItemViewModel
            {
                Title = x.ApplicationUser.FullName + " به فضای کاری پیوست",
                Icon = "fa-solid fa-user-plus",
                CreateDate = x.CreatedDate
            })
            .ToListAsync();

        var projectActivities = await _context.Projects
            .Where(x => x.WorkspaceId == workspaceId && x.ViewState)
            .OrderByDescending(x => x.CreatedDate)
            .Take(RecentItemsCount)
            .Select(x => new DashboardActivityItemViewModel
            {
                Title = "پروژه «" + x.Name + "» ایجاد شد",
                Icon = "fa-solid fa-diagram-project",
                CreateDate = x.CreatedDate
            })
            .ToListAsync();

        return memberActivities
            .Concat(projectActivities)
            .OrderByDescending(x => x.CreateDate)
            .Take(ActivityItemsCount)
            .ToList();
    }

    private async Task<(List<ChartPointViewModel> StatusChart, List<ChartPointViewModel> ActivityChart)> GetChartsDataAsync(int workspaceId)
    {
        var statusChart = await _context.Projects
            .Where(x => x.WorkspaceId == workspaceId && x.ViewState)
            .GroupBy(x => x.Status)
            .Select(g => new ChartPointViewModel
            {
                Label = g.Key.ToString(),
                Value = g.Count()
            })
            .ToListAsync();

        var fromDate = DateTime.Now.Date.AddDays(-DaysLookback);

        var memberDates = _context.WorkspaceMembers
            .Where(x => x.WorkspaceId == workspaceId && x.ViewState && x.CreatedDate >= fromDate)
            .Select(x => x.CreatedDate.Date);

        var projectDates = _context.Projects
            .Where(x => x.WorkspaceId == workspaceId && x.ViewState && x.CreatedDate >= fromDate)
            .Select(x => x.CreatedDate.Date);

        // Materialize the raw dates from both sources first (this Concat is fine — no GroupBy attached to it)
        var allDates = await memberDates.Concat(projectDates).ToListAsync();

        // Group and shape client-side
        var activityData = allDates
            .GroupBy(d => d)
            .Select(g => new ChartPointViewModel
            {
                Label = g.Key.ToString("yyyy-MM-dd"),
                Value = g.Count()
            })
            .OrderBy(x => x.Label)
            .ToList();

        return (statusChart, activityData);
    }
}
