/*
| Module      : Team
| Entity      : TeamMember
| Purpose     : مدیریت عضویت کاربران در تیم و تعیین نقش هر عضو.
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities
{
    public class TeamMember : BaseEntity
    {
        // Properties
        public int TeamId { get; set; }

        public int ApplicationUserId { get; set; }

        public TeamRoleType Role { get; set; }

        public DateTime JoinedDate { get; set; } = DateTime.Now;

        // Navigation Properties
        public Team Team { get; set; } = null!;

        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}