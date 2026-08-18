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
    private readonly INotificationService _notificationService;

    public TeamMemberService(
        IGenericRepository<TeamMember> repository,
        IUnitOfWork unitOfWork,
        ApplicationDbContext context,
        INotificationService notificationService)
        : base(repository, unitOfWork)
    {
        _context = context;
        _notificationService = notificationService;
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
        var existing = await _context.TeamMembers
            .FirstOrDefaultAsync(x => x.TeamId == teamId && x.ApplicationUserId == userId);
        if (existing != null)
        {
            if (existing.ViewState)
                return;
            existing.ViewState = true;
            existing.Role = role;
            existing.JoinedDate = DateTime.Now;
            existing.ChangeDate = DateTime.Now;
            await _context.SaveChangesAsync();

            await NotifyMemberAddedAsync(teamId, userId, role);
            return;
        }
        var member = new TeamMember
        {
            TeamId = teamId,
            ApplicationUserId = userId,
            Role = role,
            JoinedDate = DateTime.Now
        };
        _context.TeamMembers.Add(member);
        await _context.SaveChangesAsync();

        await NotifyMemberAddedAsync(teamId, userId, role);
    }
    // OPTIMIZED: Use ExecuteUpdateAsync instead of load-modify-save
    public async Task RemoveMemberAsync(int teamId, int userId)
    {
        await _context.TeamMembers
            .Where(x => x.TeamId == teamId && x.ApplicationUserId == userId && x.ViewState)
            .ExecuteUpdateAsync(u => u
                .SetProperty(x => x.ViewState, false)
                .SetProperty(x => x.ChangeDate, DateTime.Now));
    }

    // OPTIMIZED: Use ExecuteUpdateAsync instead of load-modify-save
    public async Task ChangeRoleAsync(int teamId, int userId, TeamRoleType role)
    {
        await _context.TeamMembers
            .Where(x => x.TeamId == teamId && x.ApplicationUserId == userId && x.ViewState)
            .ExecuteUpdateAsync(u => u
                .SetProperty(x => x.Role, role)
                .SetProperty(x => x.ChangeDate, DateTime.Now));
    }

    private async Task NotifyMemberAddedAsync(int teamId, int userId, TeamRoleType role)
    {
        var team = await _context.Teams.FirstOrDefaultAsync(x => x.Id == teamId);
        if (team == null)
            return;

        await _notificationService.CreateAsync(
            userId,
            "افزودن به تیم",
            $"شما با نقش «{role}» به تیم «{team.Name}» اضافه شدید.",
            NotificationType.Invitation);
    }
}