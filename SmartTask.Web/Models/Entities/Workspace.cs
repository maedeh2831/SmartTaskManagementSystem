/*
| Module      : Workspace
| Entity      : Workspace
| Purpose     : بالاترین سطح سازماندهی سامانه که شامل اعضا، تیم‌ها، پروژه‌ها و تنظیمات اختصاصی است.
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities
{
    public class Workspace : BaseEntity
    {
        // Properties
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public string? Logo { get; set; }

        public string? Color { get; set; }

        public string Slug { get; set; } = null!;

        public bool IsPublic { get; set; } = false;

        public string TimeZone { get; set; } = "Asia/Tehran";

        public LanguageType DefaultLanguage { get; set; } = LanguageType.Persian;

        // Navigation Properties
        public ICollection<WorkspaceMember> Members { get; set; } = new List<WorkspaceMember>();

        public ICollection<Team> Teams { get; set; } = new List<Team>();

        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}