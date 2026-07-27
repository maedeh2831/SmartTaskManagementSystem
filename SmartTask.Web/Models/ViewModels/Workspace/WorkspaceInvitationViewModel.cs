/*
| Module      : Workspace
| ViewModel   : WorkspaceInvitationViewModel
| Purpose     : نمایش دعوت‌نامه‌های در انتظار Workspace.
*/
using SmartTask.Web.Models.Enums;
namespace SmartTask.Web.Models.ViewModels.Workspace;
public class WorkspaceInvitationViewModel
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public WorkspaceRoleType Role { get; set; }
    public WorkspaceInvitationStatusType Status { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsNewUser { get; set; }
}