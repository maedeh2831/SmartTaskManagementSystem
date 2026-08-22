/*
| Module      : Workspace
| ViewModel   : WorkspaceMemberViewModel
| Purpose     : نمایش اعضای فضای کاری.
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Workspace;

public class WorkspaceMemberViewModel
{
    public int Id { get; set; }

    public int WorkspaceId { get; set; }

    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? Avatar { get; set; }

    public string Email { get; set; } = string.Empty;

    public WorkspaceRoleType Role { get; set; }

    public bool IsOwner { get; set; }

    public bool IsCurrentUser { get; set; }
}