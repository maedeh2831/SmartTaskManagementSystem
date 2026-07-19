/*
| Module      : Team
| Entity      : Team
| Purpose     : مدیریت تیم‌ها و سازماندهی اعضای هر Workspace.
*/

namespace SmartTask.Web.Models.Entities
{
    public class Team : BaseEntity
    {
        // Properties
        public int WorkspaceId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string? Color { get; set; }

        public string? Logo { get; set; }

        public bool IsPrivate { get; set; } = false;

        public bool IsArchived { get; set; } = false;

        // Navigation Properties
        public Workspace Workspace { get; set; } = null!;

        public ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();

        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}