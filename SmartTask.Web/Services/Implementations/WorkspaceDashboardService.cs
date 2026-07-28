using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Workspace;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations;

public class WorkspaceDashboardService : IWorkspaceDashboardService
{
    private readonly ApplicationDbContext _context;

    public WorkspaceDashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WorkspaceDashboardViewModel> GetDashboardAsync(int workspaceId, int currentUserId)
    {
        var workspace = await _context.Workspaces
            .FirstOrDefaultAsync(x => x.Id == workspaceId && x.ViewState);

        if (workspace == null)
            throw new Exception("فضای کاری یافت نشد.");

        var model = new WorkspaceDashboardViewModel
        {
            WorkspaceId = workspace.Id,
            WorkspaceName = workspace.Name,
            WorkspaceColor = workspace.Color ?? "#4F46E5"
        };

        // ===== Statistics Cards =====
        model.TotalMembers = await _context.WorkspaceMembers
            .CountAsync(x => x.WorkspaceId == workspaceId && x.ViewState);

        model.TotalProjects = await _context.Projects
            .CountAsync(x => x.WorkspaceId == workspaceId && x.ViewState);

        // فرض: Project.Status از نوع ProjectStatusType هست و مقدار Active داره
        model.ActiveProjects = await _context.Projects
            .CountAsync(x => x.WorkspaceId == workspaceId && x.ViewState
                && x.Status == ProjectStatusType.Active);

        model.PendingInvitations = await _context.WorkspaceInvitations
            .CountAsync(x => x.WorkspaceId == workspaceId
                && x.Status == WorkspaceInvitationStatusType.Pending
                && x.ExpiryDate >= DateTime.Now);

        // ===== Recent Projects =====
        model.RecentProjects = await _context.Projects
            .Where(x => x.WorkspaceId == workspaceId && x.ViewState)
            .OrderByDescending(x => x.CreatedDate)
            .Take(5)
            .Select(x => new DashboardProjectItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Color = x.Color ?? "#4F46E5",
                Status = x.Status,
                CreateDate = x.CreatedDate
            })
            .ToListAsync();

        // ===== Top Members =====
        model.TopMembers = await _context.WorkspaceMembers
            .Where(x => x.WorkspaceId == workspaceId && x.ViewState)
            .Include(x => x.ApplicationUser)
            .OrderBy(x => x.Role)
            .Take(6)
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

        // ===== Recent Activities (Proxy تا زمان پیاده‌سازی Sprint 6) =====
        var recentMemberActivities = await _context.WorkspaceMembers
            .Where(x => x.WorkspaceId == workspaceId && x.ViewState)
            .Include(x => x.ApplicationUser)
            .OrderByDescending(x => x.CreatedDate)
            .Take(5)
            .Select(x => new DashboardActivityItemViewModel
            {
                Title = x.ApplicationUser.FullName + " به فضای کاری پیوست",
                Icon = "fa-solid fa-user-plus",
                CreateDate = x.CreatedDate
            })
            .ToListAsync();

        var recentProjectActivities = await _context.Projects
            .Where(x => x.WorkspaceId == workspaceId && x.ViewState)
            .OrderByDescending(x => x.CreatedDate)
            .Take(5)
            .Select(x => new DashboardActivityItemViewModel
            {
                Title = "پروژه «" + x.Name + "» ایجاد شد",
                Icon = "fa-solid fa-diagram-project",
                CreateDate = x.CreatedDate
            })
            .ToListAsync();

        model.RecentActivities = recentMemberActivities
            .Concat(recentProjectActivities)
            .OrderByDescending(x => x.CreateDate)
            .Take(8)
            .ToList();

        // ===== Charts =====

        var statusGroups = await _context.Projects
            .Where(x => x.WorkspaceId == workspaceId && x.ViewState)
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        model.ProjectStatusChart = statusGroups
            .Select(x => new ChartPointViewModel
            {
                Label = x.Status.ToString(),
                Value = x.Count
            })
            .ToList();

        var fromDate = DateTime.Now.Date.AddDays(-6);

        var memberDates = await _context.WorkspaceMembers
            .Where(x => x.WorkspaceId == workspaceId && x.ViewState && x.CreatedDate >= fromDate)
            .Select(x => x.CreatedDate.Date)
            .ToListAsync();

        var projectDates = await _context.Projects
            .Where(x => x.WorkspaceId == workspaceId && x.ViewState && x.CreatedDate >= fromDate)
            .Select(x => x.CreatedDate.Date)
            .ToListAsync();

        var allDates = memberDates.Concat(projectDates).ToList();

        var chartPoints = new List<ChartPointViewModel>();
        for (var day = fromDate; day <= DateTime.Now.Date; day = day.AddDays(1))
        {
            chartPoints.Add(new ChartPointViewModel
            {
                Label = day.ToString("MM/dd"),
                Value = allDates.Count(d => d == day)
            });
        }

        model.ActivityChart = chartPoints;

        return model;
    }
}