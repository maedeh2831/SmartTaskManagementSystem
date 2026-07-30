/*
| Module      : Project
| Entity      : Project
| Purpose     : مدیریت اطلاعات پروژه‌ها، زمان‌بندی، وضعیت و ارتباط آن‌ها با Workspace، Team و اعضای پروژه.
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities
{
    public class Project : BaseEntity
    {
        // Properties
        public int WorkspaceId { get; set; }

        public string Name { get; set; } = null!;

        public string Key { get; set; } = null!;

        public string? Description { get; set; }

        public string? Color { get; set; }

        public string? Icon { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? DueDate { get; set; }

        public DateTime? EndDate { get; set; }

        public ProjectStatusType Status { get; set; } = ProjectStatusType.Planning;

        public ProjectPriorityType Priority { get; set; } = ProjectPriorityType.Medium;

        public bool IsArchived { get; set; }

        // Navigation Properties
        public Workspace Workspace { get; set; } = null!;

        public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();

        public ICollection<Sprint> Sprints { get; set; } = new List<Sprint>();

        public ICollection<UserStory> UserStories { get; set; } = new List<UserStory>();

        public Backlog? Backlog { get; set; }

        public ICollection<Label> Labels { get; set; } = new List<Label>();

        public ICollection<ProjectTeam> ProjectTeams { get; set; } = new List<ProjectTeam>();

    }
}