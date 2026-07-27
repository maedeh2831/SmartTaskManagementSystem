/*
| Module      : Workspace
| ViewModel   : InviteMemberViewModel
| Purpose     : دعوت کاربر جدید به Workspace.
*/

using System.ComponentModel.DataAnnotations;
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Workspace;

public class InviteMemberViewModel
{
    public int WorkspaceId { get; set; }

    [Required(ErrorMessage = "کاربر را انتخاب کنید.")]
    [Display(Name = "کاربر")]
    public int UserId { get; set; }

    [Required]
    [Display(Name = "نقش")]
    public WorkspaceRoleType Role { get; set; }
}