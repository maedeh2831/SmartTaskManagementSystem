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
    }        // OPTIMIZED: Project only WorkspaceId instead of loading full entity
        public async Task<bool> CanManageTeamAsync(int teamId, int userId)
        {
            var workspaceId = await _context.Teams
                .Where(x => x.Id == teamId)
                .Select(x => x.WorkspaceId)
                .FirstOrDefaultAsync();

            if (workspaceId <= 0) return false;

            if (await CanManageTeamsAsync(workspaceId, userId))
                return true;

            return await _context.TeamMembers
                .AnyAsync(x =>
                    x.TeamId == teamId &&
                    x.ApplicationUserId == userId &&
                    x.ViewState &&
                    x.Role == TeamRoleType.Leader);
        }

        // OPTIMIZED: Use ExecuteUpdateAsync instead of load-modify-save
        public new async Task DeleteAsync(int id)
        {
            await _context.Teams
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.ViewState, false)
                    .SetProperty(x => x.ChangeDate, DateTime.Now));
        }
}