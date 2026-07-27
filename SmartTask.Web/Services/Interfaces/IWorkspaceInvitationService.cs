using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Workspace;
namespace SmartTask.Web.Services.Interfaces;
public interface IWorkspaceInvitationService : IBaseService<WorkspaceInvitation>
{
    Task<List<WorkspaceInvitationViewModel>> GetPendingInvitationsAsync(int workspaceId);

    Task InviteAsync(
        int workspaceId,
        List<int> userIds,
        List<string> emails,
        WorkspaceRoleType role,
        int invitedByUserId);

    Task<WorkspaceInvitation?> GetByTokenAsync(Guid token);

    Task AcceptInvitationAsync(Guid token, int currentUserId);

    Task AcceptInvitationAfterRegisterAsync(Guid token, int newUserId);

    Task CancelInvitationAsync(int invitationId);
}