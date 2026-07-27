/*
| Module      : Workspace
| ViewModel   : WorkspaceMemberIndexViewModel
| Purpose     : نمایش ترکیبی اعضای فعال و دعوت‌نامه‌های در انتظار.
*/
namespace SmartTask.Web.Models.ViewModels.Workspace;
public class WorkspaceMemberIndexViewModel
{
    public int WorkspaceId { get; set; }
    public bool CanManage { get; set; }
    public List<WorkspaceMemberViewModel> Members { get; set; } = new();
    public List<WorkspaceInvitationViewModel> Invitations { get; set; } = new();
}