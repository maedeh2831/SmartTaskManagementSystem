using System.ComponentModel.DataAnnotations;
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.ProjectMember;

public class InviteProjectMemberViewModel
{
    [Required]
    public int ProjectId { get; set; }

    [Required(ErrorMessage = "انتخاب عضو الزامی است.")]
    [Display(Name = "عضو")]
    public int ApplicationUserId { get; set; }

    [Display(Name = "نقش")]
    public ProjectRoleType Role { get; set; } = ProjectRoleType.Developer;
}