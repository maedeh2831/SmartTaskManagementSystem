/*
| Module      : Workspace
| Entity      : WorkspaceMember
| Purpose     : مدیریت عضویت کاربران در Workspace و تعیین نقش هر عضو.
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities
{
    public class WorkspaceMember : BaseEntity
    {
        // Properties
        public int WorkspaceId { get; set; }

        public int ApplicationUserId { get; set; }

        public WorkspaceRoleType Role { get; set; }

        // Navigation Properties
        public Workspace Workspace { get; set; } = null!;

        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}