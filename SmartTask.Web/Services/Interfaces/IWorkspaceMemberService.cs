using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Workspace;

namespace SmartTask.Web.Services.Interfaces;

public interface IWorkspaceMemberService
    : IBaseService<WorkspaceMember>
{
    Task<List<WorkspaceMemberViewModel>> GetMembersAsync(int workspaceId);

    Task InviteMemberAsync(
        int workspaceId,
        int userId,
        WorkspaceRoleType role);

    Task RemoveMemberAsync(int memberId);

    Task ChangeRoleAsync(
        int memberId,
        WorkspaceRoleType role);

    Task<bool> IsMemberAsync(
        int workspaceId,
        int userId);

    Task<bool> IsOwnerOrAdminAsync(
        int workspaceId,
        int userId);
}