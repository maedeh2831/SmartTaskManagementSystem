using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.ProjectMember;

public class ProjectMemberViewModel
{
    public int ApplicationUserId { get; set; }
    public string FullName { get; set; } = null!;
    public ProjectRoleType Role { get; set; }
    public DateTime JoinedDate { get; set; }
}