/*
| Module      : Agile
| Entity      : Sprint
| Purpose     : مدیریت اسپرینت‌های هر پروژه و برنامه‌ریزی اجرای User Storyها.
*/

using Microsoft.AspNetCore.Identity;
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities
{
    public class Sprint : BaseEntity
    {
        // Properties

        public int ProjectId { get; set; }

        public string Name { get; set; } = null!;

        public string Goal { get; set; } = null!;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public SprintStatusType Status { get; set; } = SprintStatusType.Planned;

        public bool IsCompleted { get; set; } = false;

        // Navigation Properties

        public Project Project { get; set; } = null!;

        public ICollection<UserStory> UserStories { get; set; } = new List<UserStory>();
    }
}