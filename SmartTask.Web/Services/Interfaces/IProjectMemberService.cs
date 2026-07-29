using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Services.Interfaces;

public interface IProjectMemberService
{
    Task<bool> IsMemberAsync(int projectId, int userId);

    Task AddMemberAsync(int projectId, int userId, ProjectRoleType role);

    Task RemoveMemberAsync(int projectId, int userId);

    Task ChangeRoleAsync(int projectId, int userId, ProjectRoleType role);
}