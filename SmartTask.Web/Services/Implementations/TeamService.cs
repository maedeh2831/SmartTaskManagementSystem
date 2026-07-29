using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations;

public class TeamService : BaseService<Team>, ITeamService
{
    private readonly ApplicationDbContext _context;

    public TeamService(
        IGenericRepository<Team> repository,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context)
        : base(repository, unitOfWork)
    {
        _context = context;
    }

    public async Task<Team?> GetDetailsAsync(int id)
    {
        return await _context.Teams
            .Include(x => x.Members.Where(m => m.ViewState))
                .ThenInclude(m => m.ApplicationUser)
            .Include(x => x.ProjectTeams.Where(pt => pt.ViewState))
                .ThenInclude(pt => pt.Project)
            .FirstOrDefaultAsync(x => x.Id == id && x.ViewState);
    }

    public async Task<bool> ExistsByNameAsync(
        int workspaceId,
        string name,
        int? excludeId = null)
    {
        var query = _repository
            .Query()
            .Where(x => x.WorkspaceId == workspaceId && x.Name == name);

        if (excludeId.HasValue)
            query = query.Where(x => x.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> CanManageTeamsAsync(int workspaceId, int userId)
    {
        var isOwner = await _context.Workspaces
            .AnyAsync(x => x.Id == workspaceId && x.OwnerId == userId);

        if (isOwner)
            return true;

        return await _context.WorkspaceMembers
            .AnyAsync(x =>
                x.WorkspaceId == workspaceId &&
                x.ApplicationUserId == userId &&
                x.ViewState &&
                (x.Role == WorkspaceRoleType.Owner || x.Role == WorkspaceRoleType.Admin));
    }

    public async Task<bool> CanManageTeamAsync(int teamId, int userId)
    {
        var team = await _repository
            .Query()
            .FirstOrDefaultAsync(x => x.Id == teamId);

        if (team == null)
            return false;

        if (await CanManageTeamsAsync(team.WorkspaceId, userId))
            return true;

        return await _context.TeamMembers
            .AnyAsync(x =>
                x.TeamId == teamId &&
                x.ApplicationUserId == userId &&
                x.ViewState &&
                x.Role == TeamRoleType.Leader);
    }

    public new async Task DeleteAsync(int id)
    {
        var team = await _context.Teams
            .FirstOrDefaultAsync(x => x.Id == id);

        if (team == null)
            return;

        team.ViewState = false;
        await _context.SaveChangesAsync();
    }
}