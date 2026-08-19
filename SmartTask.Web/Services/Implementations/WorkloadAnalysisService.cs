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
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
    }

    public async Task<WorkloadIndexViewModel?> GetWorkloadAsync(int projectId, int currentUserId)
    {
        if (projectId <= 0 || currentUserId <= 0)
            return null;

        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == projectId && x.ViewState);

        if (project == null)
            return null;

        // Load members once
        var members = await _context.ProjectMembers
            .Where(x => x.ProjectId == projectId && x.ViewState)
            .Include(x => x.ApplicationUser)
            .ToListAsync();

        var activeSprint = await _context.Sprints
            .FirstOrDefaultAsync(x => x.ProjectId == projectId
                && x.Status == SprintStatusType.Active && x.ViewState);

        // OPTIMIZED: Single query for all open tasks with assignments
        var openTasksQuery = _context.TaskItems
            .Where(t => t.UserStory.ProjectId == projectId
                && t.ViewState
                && t.Status != TaskStatusType.Done
                && t.Status != TaskStatusType.Cancelled)
            .Include(t => t.Assignments)
            .Include(t => t.UserStory)
            .AsQueryable();

        var projectTasks = await openTasksQuery.ToListAsync();

        // OPTIMIZED: Filter in-memory instead of second DB query
        var sprintTasks = activeSprint == null
            ? new List<Models.Entities.TaskItem>()
            : projectTasks
                .Where(t => t.UserStory.SprintId == activeSprint.Id)
                .ToList();

        // Pre-compute assignment mappings to avoid O(n²) lookups
        var projectAssignmentMap = ComputeAssignmentMap(projectTasks);
        var sprintAssignmentMap = ComputeAssignmentMap(sprintTasks);

        var vm = new WorkloadIndexViewModel
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            CanManage = await _projectService.CanManageProjectAsync(projectId, currentUserId),
            HasActiveSprint = activeSprint != null,
            ActiveSprintName = activeSprint?.Name,
            ActiveSprintEndDate = activeSprint?.EndDate,
            ProjectWorkload = BuildWorkloadList(members, projectTasks, projectAssignmentMap),
            SprintWorkload = BuildWorkloadList(members, sprintTasks, sprintAssignmentMap),
            ProjectUnassignedHours = projectTasks
                .Where(t => !t.Assignments.Any())
                .Sum(t => (double)t.Estimate),
            SprintUnassignedHours = sprintTasks
                .Where(t => !t.Assignments.Any())
                .Sum(t => (double)t.Estimate)
        };

        return vm;
    }

    /// <summary>
    /// OPTIMIZED: Pre-compute assignment map to avoid O(n²) nested filtering
    /// Maps userId -> list of task estimates
    /// </summary>
    private static Dictionary<int, List<double>> ComputeAssignmentMap(List<Models.Entities.TaskItem> tasks)
    {
        var map = new Dictionary<int, List<double>>();

        foreach (var task in tasks)
        {
            if (task.Assignments == null || task.Assignments.Count == 0)
                continue;

            var estimatePerAssignee = (double)task.Estimate / task.Assignments.Count;

            foreach (var assignment in task.Assignments)
            {
                if (!map.ContainsKey(assignment.ApplicationUserId))
                    map[assignment.ApplicationUserId] = new List<double>();

                map[assignment.ApplicationUserId].Add(estimatePerAssignee);
            }
        }

        return map;
    }

    /// <summary>
    /// OPTIMIZED: Use pre-computed assignment map to eliminate nested loops
    /// </summary>
    private static List<WorkloadMemberViewModel> BuildWorkloadList(
        List<Models.Entities.ProjectMember> members,
        List<Models.Entities.TaskItem> tasks,
        Dictionary<int, List<double>> assignmentMap)
    {
        var result = new List<WorkloadMemberViewModel>();

        foreach (var member in members)
        {
            // Get assigned hours from pre-computed map (O(1) lookup)
            double assignedHours = 0;
            int taskCount = 0;

            if (assignmentMap.TryGetValue(member.ApplicationUserId, out var estimates))
            {
                assignedHours = estimates.Sum();
                taskCount = estimates.Count;
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
                TaskCount = taskCount,
                UtilizationPercent = utilization,
                StatusLevel = statusLevel
            });
        }

        return result.OrderByDescending(x => x.UtilizationPercent).ToList();
    }

    public async Task UpdateCapacityAsync(int projectMemberId, int weeklyCapacityHours)
    {
        if (projectMemberId <= 0)
            return;

        var member = await _context.ProjectMembers
            .FirstOrDefaultAsync(x => x.Id == projectMemberId && x.ViewState);
        if (member == null) return;

        member.WeeklyCapacityHours = Math.Max(1, weeklyCapacityHours);
        member.ChangeDate = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// OPTIMIZED: Single query with server-side aggregation instead of loading all tasks
    /// </summary>
    public async Task<int> GetUserUtilizationAsync(int projectId, int userId)
    {
        if (projectId <= 0 || userId <= 0)
            return 0;

        var member = await _context.ProjectMembers
            .Where(x => x.ProjectId == projectId && x.ApplicationUserId == userId && x.ViewState)
            .Select(x => x.WeeklyCapacityHours)
            .FirstOrDefaultAsync();

        if (member <= 0)
            return 0;

        // OPTIMIZED: Server-side aggregation - single query instead of load-then-calculate
        var assignedHours = await _context.TaskItems
            .Where(t => t.UserStory.ProjectId == projectId
                && t.ViewState
                && t.Status != TaskStatusType.Done
                && t.Status != TaskStatusType.Cancelled
                && t.Assignments.Any(a => a.ApplicationUserId == userId))
            .Select(t => new { t.Estimate, AssigneeCount = t.Assignments.Count })
            .ToListAsync();

        if (assignedHours.Count == 0)
            return 0;

        // Calculate total assigned hours
        var totalHours = assignedHours.Sum(t => (double)t.Estimate / Math.Max(1, t.AssigneeCount));
        var capacity = member <= 0 ? 1 : member;

        return (int)Math.Round(totalHours / capacity * 100);
    }
}
