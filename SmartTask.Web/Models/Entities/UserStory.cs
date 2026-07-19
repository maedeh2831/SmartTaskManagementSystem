/*
| Module      : Agile
| Entity      : UserStory
| Purpose     : مدیریت نیازمندی‌های پروژه به صورت User Story.
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities
{
    public class UserStory : BaseEntity
    {
        // Properties

        public int ProjectId { get; set; }

        public int BacklogId { get; set; }

        public int? SprintId { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        public int StoryPoint { get; set; }

        public StoryPriorityType Priority { get; set; } = StoryPriorityType.Medium;

        public StoryStatusType Status { get; set; } = StoryStatusType.New;

        // Navigation Properties

        public Project Project { get; set; } = null!;

        public Backlog Backlog { get; set; } = null!;

        public Sprint? Sprint { get; set; }

        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}