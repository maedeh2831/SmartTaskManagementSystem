using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Interfaces;
using SmartTask.Web.Models.DTOs;

namespace SmartTask.Web.Services.Implementations;

public class SprintService : BaseService<Sprint>, ISprintService
{
    private readonly ApplicationDbContext _context;

    public SprintService(
        IGenericRepository<Sprint> repository,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context)
        : base(repository, unitOfWork)
    {
        _context = context;
    }

    public async Task<Sprint?> GetDetailsAsync(int id)
    {
        return await _context.Sprints
            .Include(x => x.Project)
            .Include(x => x.UserStories.Where(s => s.ViewState))
            .FirstOrDefaultAsync(x => x.Id == id && x.ViewState);
    }

    public async Task<List<Sprint>> GetByProjectAsync(int projectId)
    {
        return await _context.Sprints
            .Where(x => x.ProjectId == projectId && x.ViewState)
            .Include(x => x.UserStories.Where(s => s.ViewState))
            .OrderByDescending(x => x.StartDate)
            .ToListAsync();
    }

    public async Task<bool> ExistsByNameAsync(
        int projectId,
        string name,
        int? excludeId = null)
    {
        var query = _repository
            .Query()
            .Where(x => x.ProjectId == projectId && x.Name == name);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> HasDateOverlapAsync(
        int projectId,
        DateTime startDate,
        DateTime endDate,
        int? excludeId = null)
    {
        var query = _repository
            .Query()
            .Where(x =>
                x.ProjectId == projectId &&
                x.Status != SprintStatusType.Completed &&
                x.Status != SprintStatusType.Cancelled &&
                x.StartDate < endDate &&
                x.EndDate > startDate);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> CanManageSprintsAsync(int projectId, int userId)
    {
        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == projectId);

        if (project == null)
            return false;

        var isWorkspaceOwner = await _context.Workspaces
            .AnyAsync(x => x.Id == project.WorkspaceId && x.OwnerId == userId);

        if (isWorkspaceOwner)
            return true;

        return await _context.WorkspaceMembers
            .AnyAsync(x =>
                x.WorkspaceId == project.WorkspaceId &&
                x.ApplicationUserId == userId &&
                x.ViewState &&
                (x.Role == WorkspaceRoleType.Owner || x.Role == WorkspaceRoleType.Admin));
    }

    public async Task<bool> CanManageSprintAsync(int sprintId, int userId)
    {
        var sprint = await _repository
            .Query()
            .FirstOrDefaultAsync(x => x.Id == sprintId);

        if (sprint == null)
            return false;

        return await CanManageSprintsAsync(sprint.ProjectId, userId);
    }

    public async Task ActivateAsync(int sprintId)
    {
        var sprint = await _context.Sprints
            .FirstOrDefaultAsync(x => x.Id == sprintId);

        if (sprint == null)
            return;

        // فقط یک اسپرینت فعال در هر پروژه مجاز است
        var otherActive = await _context.Sprints
            .Where(x =>
                x.ProjectId == sprint.ProjectId &&
                x.Id != sprintId &&
                x.Status == SprintStatusType.Active)
            .ToListAsync();

        foreach (var s in otherActive)
            s.Status = SprintStatusType.Planning;

        sprint.Status = SprintStatusType.Active;
        sprint.ChangeDate = DateTime.Now;

        await _context.SaveChangesAsync();
    }

    public async Task CompleteAsync(int sprintId)
    {
        var sprint = await _context.Sprints
            .FirstOrDefaultAsync(x => x.Id == sprintId);

        if (sprint == null)
            return;

        sprint.Status = SprintStatusType.Completed;
        sprint.ChangeDate = DateTime.Now;

        await _context.SaveChangesAsync();
    }

    public new async Task DeleteAsync(int id)
    {
        var sprint = await _context.Sprints
            .FirstOrDefaultAsync(x => x.Id == id);

        if (sprint == null)
            return;

        sprint.ViewState = false;
        await _context.SaveChangesAsync();
    }

    public async Task<List<BurndownPointDto>> GetBurndownDataAsync(int sprintId)
    {
        var sprint = await _context.Sprints
            .Include(x => x.UserStories.Where(s => s.ViewState))
            .FirstOrDefaultAsync(x => x.Id == sprintId);

        if (sprint == null)
            return new List<BurndownPointDto>();

        var totalPoints = sprint.UserStories.Sum(x => x.StoryPoint);
        var totalDays = Math.Max(1, (sprint.EndDate.Date - sprint.StartDate.Date).Days);
        var today = DateTime.Today;

        var doneStories = sprint.UserStories
            .Where(x => x.Status == StoryStatusType.Done)
            .Select(x => new { x.StoryPoint, CompletedOn = (x.ChangeDate ?? x.CreatedDate).Date })
            .ToList();

        var points = new List<BurndownPointDto>();

        for (var day = sprint.StartDate.Date; day <= sprint.EndDate.Date; day = day.AddDays(1))
        {
            var elapsedDays = (day - sprint.StartDate.Date).Days;
            var idealRemaining = (int)Math.Round(
                totalPoints - ((double)totalPoints / totalDays * elapsedDays));

            int? actualRemaining = null;

            if (day <= today)
            {
                var completedByThisDay = doneStories
                    .Where(x => x.CompletedOn <= day)
                    .Sum(x => x.StoryPoint);

                actualRemaining = totalPoints - completedByThisDay;
            }

            points.Add(new BurndownPointDto
            {
                Date = day,
                IdealRemaining = Math.Max(0, idealRemaining),
                ActualRemaining = actualRemaining
            });
        }

        return points;
    }

    public async Task<List<VelocityPointDto>> GetVelocityDataAsync(int projectId, int lastCount = 6)
    {
        var completedSprints = await _context.Sprints
            .Where(x => x.ProjectId == projectId && x.ViewState && x.Status == SprintStatusType.Completed)
            .Include(x => x.UserStories.Where(s => s.ViewState))
            .OrderByDescending(x => x.EndDate)
            .Take(lastCount)
            .ToListAsync();

        completedSprints.Reverse(); // ترتیب زمانی صحیح برای نمودار

        return completedSprints.Select(sprint => new VelocityPointDto
        {
            SprintName = sprint.Name,
            PlannedPoints = sprint.UserStories.Sum(x => x.StoryPoint),
            CompletedPoints = sprint.UserStories
                .Where(x => x.Status == StoryStatusType.Done)
                .Sum(x => x.StoryPoint)
        }).ToList();
    }
}