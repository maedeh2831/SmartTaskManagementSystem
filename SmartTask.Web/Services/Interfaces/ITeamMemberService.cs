using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Services.Interfaces;

public interface ITeamMemberService : IBaseService<TeamMember>
{
    Task<bool> IsMemberAsync(int teamId, int userId);

    Task AddMemberAsync(int teamId, int userId, TeamRoleType role);

    Task RemoveMemberAsync(int teamId, int userId);

    Task ChangeRoleAsync(int teamId, int userId, TeamRoleType role);
}