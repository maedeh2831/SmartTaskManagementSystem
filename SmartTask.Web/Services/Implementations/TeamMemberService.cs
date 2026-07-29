using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations;

public class TeamMemberService : BaseService<TeamMember>, ITeamMemberService
{
    private readonly ApplicationDbContext _context;

    public TeamMemberService(
        IGenericRepository<TeamMember> repository,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context)
        : base(repository, unitOfWork)
    {
        _context = context;
    }

    public async Task<bool> IsMemberAsync(int teamId, int userId)
    {
        return await _context.TeamMembers
            .AnyAsync(x =>
                x.TeamId == teamId &&
                x.ApplicationUserId == userId &&
                x.ViewState);
    }

    public async Task AddMemberAsync(int teamId, int userId, TeamRoleType role)
    {
        if (await IsMemberAsync(teamId, userId))
            return;

        var member = new TeamMember
        {
            TeamId = teamId,
            ApplicationUserId = userId,
            Role = role,
            JoinedDate = DateTime.Now
        };

        _context.TeamMembers.Add(member);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(int teamId, int userId)
    {
        var member = await _context.TeamMembers
            .FirstOrDefaultAsync(x =>
                x.TeamId == teamId &&
                x.ApplicationUserId == userId &&
                x.ViewState);

        if (member == null)
            return;

        member.ViewState = false;
        await _context.SaveChangesAsync();
    }

    public async Task ChangeRoleAsync(int teamId, int userId, TeamRoleType role)
    {
        var member = await _context.TeamMembers
            .FirstOrDefaultAsync(x =>
                x.TeamId == teamId &&
                x.ApplicationUserId == userId &&
                x.ViewState);

        if (member == null)
            return;

        member.Role = role;
        member.ChangeDate = DateTime.Now;
        await _context.SaveChangesAsync();
    }
}