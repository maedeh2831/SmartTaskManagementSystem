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
    private readonly SmartTask.Web.Services.Gamification.ITaskRewardCoordinator _rewardCoordinator;

    public SprintService(
        IGenericRepository<Sprint> repository,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context,
        SmartTask.Web.Services.Gamification.ITaskRewardCoordinator rewardCoordinator)
        : base(repository, unitOfWork)
    {
        _context = context;
        _rewardCoordinator = rewardCoordinator;
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
        var query = _repository.Query()
            .Where(x => x.ProjectId == projectId && x.Name == name && x.ViewState);

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
        var query = _repository.Query()
            .Where(x =>
                x.ProjectId == projectId &&
                x.ViewState &&
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
        return await _context.Projects
            .Where(p => p.Id == projectId)
            .AnyAsync(p =>
                p.Workspace.OwnerId == userId ||
                p.Workspace.Members.Any(m =>
                    m.ApplicationUserId == userId &&
                    m.ViewState &&
                    (m.Role == WorkspaceRoleType.Owner || m.Role == WorkspaceRoleType.Admin)));
    }

    public async Task<bool> CanManageSprintAsync(int sprintId, int userId)
    {
        var projectId = await _repository.Query()
            .Where(x => x.Id == sprintId)
            .Select(x => x.ProjectId)
            .FirstOrDefaultAsync();

        if (projectId == 0)
            return false;

        return await CanManageSprintsAsync(projectId, userId);
    }

    public async Task ActivateAsync(int sprintId)
    {
        var sprint = await _context.Sprints
            .FirstOrDefaultAsync(x => x.Id == sprintId);

        if (sprint == null)
            return;

        var otherActiveSprints = await _context.Sprints
            .Where(x =>
                x.ProjectId == sprint.ProjectId &&
                x.Id != sprintId &&
                x.Status == SprintStatusType.Active)
            .ToListAsync();

        foreach (var other in otherActiveSprints)
        {
            other.Status = SprintStatusType.Planning;
            other.ChangeDate = DateTime.Now;
        }

        sprint.Status = SprintStatusType.Active;
        sprint.ChangeDate = DateTime.Now;

        await _context.SaveChangesAsync();
    }

    public async Task CompleteAsync(int sprintId)
    {
        var sprint = await _context.Sprints.FirstOrDefaultAsync(x => x.Id == sprintId);
        if (sprint == null) return;

        // فقط اولین بار پاداش داده می‌شود
        var wasAlreadyCompleted = sprint.Status == SprintStatusType.Completed;

        sprint.Status = SprintStatusType.Completed;
        sprint.ChangeDate = DateTime.Now;
        await _context.SaveChangesAsync();

        if (!wasAlreadyCompleted)
            await _rewardCoordinator.HandleSprintCompletedAsync(sprintId);
    }

    public new async Task DeleteAsync(int id)
    {
        var sprint = await _context.Sprints.FirstOrDefaultAsync(x => x.Id == id);
        if (sprint == null) return;

        sprint.ViewState = false;
        await _context.SaveChangesAsync();
    }

    public async Task<List<BurndownPointDto>> GetBurndownDataAsync(int sprintId)
    {
        var sprint = await _context.Sprints
            .Select(s => new
            {
                s.Id,
                s.StartDate,
                s.EndDate,
                TotalPoints = s.UserStories.Where(us => us.ViewState).Sum(us => us.StoryPoint)
            })
            .FirstOrDefaultAsync(x => x.Id == sprintId);

        if (sprint == null)
            return new List<BurndownPointDto>();

        var doneStories = await _context.UserStories
            .Where(x => x.SprintId == sprintId && x.ViewState && x.Status == StoryStatusType.Done)
            .Select(x => new { x.StoryPoint, CompletedOn = (x.ChangeDate ?? x.CreatedDate).Date })
            .ToListAsync();

        var points = new List<BurndownPointDto>();
        var totalDays = Math.Max(1, (sprint.EndDate.Date - sprint.StartDate.Date).Days);
        var today = DateTime.Today;

        for (var day = sprint.StartDate.Date; day <= sprint.EndDate.Date; day = day.AddDays(1))
        {
            var elapsedDays = (day - sprint.StartDate.Date).Days;
            var idealRemaining = (int)Math.Round(
                sprint.TotalPoints - ((double)sprint.TotalPoints / totalDays * elapsedDays));

            int? actualRemaining = null;

            if (day <= today)
            {
                var completedByThisDay = doneStories
                    .Where(x => x.CompletedOn <= day)
                    .Sum(x => x.StoryPoint);

                actualRemaining = sprint.TotalPoints - completedByThisDay;
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
            .OrderByDescending(x => x.EndDate)
            .Take(lastCount)
            .Select(sprint => new VelocityPointDto
            {
                SprintName = sprint.Name,
                PlannedPoints = sprint.UserStories.Where(s => s.ViewState).Sum(s => s.StoryPoint),
                CompletedPoints = sprint.UserStories
                    .Where(s => s.ViewState && s.Status == StoryStatusType.Done)
                    .Sum(s => s.StoryPoint)
            })
            .ToListAsync();

        // Reverse so chart shows oldest→newest (left→right)
        completedSprints.Reverse();
        return completedSprints;
    }
}
