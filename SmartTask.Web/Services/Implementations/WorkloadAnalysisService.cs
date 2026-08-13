using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Workload;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations;

public class WorkloadAnalysisService : IWorkloadAnalysisService
{
    private readonly ApplicationDbContext _context;
    private readonly IProjectService _projectService;

    public WorkloadAnalysisService(ApplicationDbContext context, IProjectService projectService)
    {
        _context = context;
        _projectService = projectService;
    }

    public async Task<WorkloadIndexViewModel?> GetWorkloadAsync(int projectId, int currentUserId)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == projectId && x.ViewState);
        if (project == null)
            return null;

        var members = await _context.ProjectMembers
            .Where(x => x.ProjectId == projectId && x.ViewState)
            .Include(x => x.ApplicationUser)
            .ToListAsync();

        var activeSprint = await _context.Sprints
            .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.Status == SprintStatusType.Active && x.ViewState);

        // ===== تسک‌های باز کل پروژه (به همراه Assignee ها) =====
        var openTasksQuery = _context.TaskItems
            .Where(t => t.UserStory.ProjectId == projectId
                && t.ViewState
                && t.Status != TaskStatusType.Done
                && t.Status != TaskStatusType.Cancelled)
            .Include(t => t.Assignments)
            .AsQueryable();

        var projectTasks = await openTasksQuery.ToListAsync();

        var sprintTasks = activeSprint == null
            ? new List<Models.Entities.TaskItem>()
            : await openTasksQuery
                .Where(t => t.UserStory.SprintId == activeSprint.Id)
                .ToListAsync();

        var vm = new WorkloadIndexViewModel
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            CanManage = await _projectService.CanManageProjectAsync(projectId, currentUserId),
            HasActiveSprint = activeSprint != null,
            ActiveSprintName = activeSprint?.Name,
            ActiveSprintEndDate = activeSprint?.EndDate,
            ProjectWorkload = BuildWorkloadList(members, projectTasks),
            SprintWorkload = BuildWorkloadList(members, sprintTasks),
            ProjectUnassignedHours = projectTasks.Where(t => !t.Assignments.Any()).Sum(t => (double)t.Estimate),
            SprintUnassignedHours = sprintTasks.Where(t => !t.Assignments.Any()).Sum(t => (double)t.Estimate)
        };

        return vm;
    }

    private static List<WorkloadMemberViewModel> BuildWorkloadList(
        List<Models.Entities.ProjectMember> members,
        List<Models.Entities.TaskItem> tasks)
    {
        var result = new List<WorkloadMemberViewModel>();

        foreach (var member in members)
        {
            var myTasks = tasks
                .Where(t => t.Assignments.Any(a => a.ApplicationUserId == member.ApplicationUserId))
                .ToList();

            double assignedHours = 0;
            foreach (var task in myTasks)
            {
                var assigneeCount = task.Assignments.Count;
                assignedHours += assigneeCount > 0 ? (double)task.Estimate / assigneeCount : 0;
            }

            var capacity = member.WeeklyCapacityHours <= 0 ? 1 : member.WeeklyCapacityHours;
            var utilization = (int)Math.Round(assignedHours / capacity * 100);

            var statusLevel = utilization switch
            {
                < 80 => "under",
                <= 100 => "balanced",
                _ => "overloaded"
            };

            result.Add(new WorkloadMemberViewModel
            {
                ProjectMemberId = member.Id,
                UserId = member.ApplicationUserId,
                FullName = member.ApplicationUser.FullName,
                Role = member.Role,
                CapacityHours = member.WeeklyCapacityHours,
                AssignedHours = Math.Round(assignedHours, 1),
                TaskCount = myTasks.Count,
                UtilizationPercent = utilization,
                StatusLevel = statusLevel
            });
        }

        return result.OrderByDescending(x => x.UtilizationPercent).ToList();
    }

    public async Task UpdateCapacityAsync(int projectMemberId, int weeklyCapacityHours)
    {
        var member = await _context.ProjectMembers.FirstOrDefaultAsync(x => x.Id == projectMemberId);
        if (member == null) return;

        member.WeeklyCapacityHours = Math.Max(1, weeklyCapacityHours);
        member.ChangeDate = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetUserUtilizationAsync(int projectId, int userId)
    {
        var member = await _context.ProjectMembers
            .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.ApplicationUserId == userId && x.ViewState);

        if (member == null)
            return 0;

        var openTasks = await _context.TaskItems
            .Where(t => t.UserStory.ProjectId == projectId
                && t.ViewState
                && t.Status != TaskStatusType.Done
                && t.Status != TaskStatusType.Cancelled
                && t.Assignments.Any(a => a.ApplicationUserId == userId))
            .Include(t => t.Assignments)
            .ToListAsync();

        double assignedHours = 0;
        foreach (var task in openTasks)
        {
            var assigneeCount = task.Assignments.Count;
            assignedHours += assigneeCount > 0 ? (double)task.Estimate / assigneeCount : 0;
        }

        var capacity = member.WeeklyCapacityHours <= 0 ? 1 : member.WeeklyCapacityHours;
        return (int)Math.Round(assignedHours / capacity * 100);
    }
}