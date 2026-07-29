using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Team;

public class TeamMemberViewModel
{
    public int ApplicationUserId { get; set; }
    public string FullName { get; set; } = null!;
    public TeamRoleType Role { get; set; }
    public DateTime JoinedDate { get; set; }
}