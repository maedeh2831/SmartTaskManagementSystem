/*
| Module      : Project
| Entity      : Sprint
| Purpose     : مدیریت اسپرینت‌های Agile در هر پروژه.
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities
{
    public class Sprint : BaseEntity
    {
        // Properties
        public int ProjectId { get; set; }

        public string Name { get; set; } = null!;

        public string? Goal { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public int Capacity { get; set; }

        public SprintStatusType Status { get; set; } = SprintStatusType.Planning;

        // Navigation Properties
        public Project Project { get; set; } = null!;

        public ICollection<UserStory> UserStories { get; set; } = new List<UserStory>();
    }
}