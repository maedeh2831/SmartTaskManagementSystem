/*
| Module      : Project
| Entity      : ProjectMember
| Purpose     : مدیریت عضویت کاربران در پروژه و تعیین نقش هر عضو.
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities
{
    public class ProjectMember : BaseEntity
    {
        // Properties
        public int ProjectId { get; set; }

        public int ApplicationUserId { get; set; }

        public ProjectRoleType Role { get; set; }

        public DateTime JoinedDate { get; set; } = DateTime.Now;

        // Navigation Properties
        public Project Project { get; set; } = null!;

        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}