using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.ProjectDashboard;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations;

public class ProjectDashboardService : IProjectDashboardService
{
    private readonly ApplicationDbContext _context;

    public ProjectDashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProjectDashboardViewModel?> GetDashboardAsync(int projectId)
    {
        var project = await _context.Projects
            .Include(x => x.Members.Where(m => m.ViewState))
                .ThenInclude(m => m.ApplicationUser)
            .Include(x => x.ProjectTeams.Where(pt => pt.ViewState))
                .ThenInclude(pt => pt.Team)
            .FirstOrDefaultAsync(x => x.Id == projectId && x.ViewState);

        if (project == null)
            return null;

        var now = DateTime.Now;

        // ===== Progress Calculation (Placeholder تا Task/Sprint واقعی پیاده بشه) =====
        int progressPercentage;

        if (project.Status == ProjectStatusType.Completed)
        {
            progressPercentage = 100;
        }
        else if (project.Status == ProjectStatusType.Cancelled)
        {
            progressPercentage = 0;
        }
        else if (project.StartDate.HasValue && project.DueDate.HasValue && project.DueDate > project.StartDate)
        {
            var totalDays = (project.DueDate.Value - project.StartDate.Value).TotalDays;
            var elapsedDays = (now - project.StartDate.Value).TotalDays;
            progressPercentage = (int)Math.Clamp((elapsedDays / totalDays) * 100, 0, 100);
        }
        else
        {
            progressPercentage = project.Status switch
            {
                ProjectStatusType.Planning => 10,
                ProjectStatusType.Active => 50,
                ProjectStatusType.OnHold => 40,
                _ => 0
            };
        }

        // ===== Timeline =====
        int? daysRemaining = null;
        var isOverdue = false;

        if (project.DueDate.HasValue)
        {
            daysRemaining = (project.DueDate.Value.Date - now.Date).Days;
            isOverdue = daysRemaining < 0 &&
                project.Status != ProjectStatusType.Completed &&
                project.Status != ProjectStatusType.Cancelled;
        }

        // ===== Member Workload (بر اساس توزیع نقش) =====
        var roleDistribution = project.Members
            .GroupBy(m => m.Role)
            .Select(g => new RoleDistributionItem
            {
                Role = g.Key,
                Count = g.Count()
            })
            .OrderBy(x => x.Role)
            .ToList();

        // ===== Recent Activity (فعلاً بر اساس آخرین اعضای اضافه‌شده) =====
        var recentMembers = project.Members
            .OrderByDescending(m => m.JoinedDate)
            .Take(5)
            .Select(m => new RecentMemberViewModel
            {
                FullName = m.ApplicationUser.FullName,
                Role = m.Role,
                JoinedDate = m.JoinedDate
            })
            .ToList();

        return new ProjectDashboardViewModel
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            ProjectKey = project.Key,
            Color = project.Color ?? "#4F46E5",
            Icon = project.Icon ?? "fa-solid fa-diagram-project",
            Status = project.Status,
            Priority = project.Priority,
            StartDate = project.StartDate,
            DueDate = project.DueDate,
            DaysRemaining = daysRemaining,
            IsOverdue = isOverdue,
            ProgressPercentage = progressPercentage,
            MembersCount = project.Members.Count,
            TeamsCount = project.ProjectTeams.Count,
            RoleDistribution = roleDistribution,
            RecentMembers = recentMembers,
            TeamNames = project.ProjectTeams.Select(pt => pt.Team.Name).ToList()
        };
    }
}