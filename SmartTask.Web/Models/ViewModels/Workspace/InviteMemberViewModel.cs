/*
| Module      : Workspace
| ViewModel   : InviteMemberViewModel
| Purpose     : دعوت چندین کاربر (عضو یا ایمیل جدید) به Workspace.
*/
using System.ComponentModel.DataAnnotations;
using SmartTask.Web.Models.Enums;
namespace SmartTask.Web.Models.ViewModels.Workspace;
public class InviteMemberViewModel
{
    public int WorkspaceId { get; set; }

    public List<int> UserIds { get; set; } = new();

    public List<string> Emails { get; set; } = new();

    [Required]
    [Display(Name = "نقش")]
    public WorkspaceRoleType Role { get; set; }
}