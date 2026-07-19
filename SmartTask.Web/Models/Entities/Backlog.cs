/*
| Module      : Agile
| Entity      : Backlog
| Purpose     : نگهداری User Storyهای برنامه‌ریزی نشده هر پروژه.
*/

using Microsoft.AspNetCore.Identity;

namespace SmartTask.Web.Models.Entities
{
    public class Backlog : BaseEntity
    {
        // Properties

        public int ProjectId { get; set; }

        public string Name { get; set; } = "Product Backlog";

        public string? Description { get; set; }

        // Navigation Properties

        public Project Project { get; set; } = null!;

        public ICollection<UserStory> UserStories { get; set; } = new List<UserStory>();
    }
}