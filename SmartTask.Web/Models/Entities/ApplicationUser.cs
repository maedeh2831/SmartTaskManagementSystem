/*
| Module      : Identity
| Entity      : ApplicationUser
| Purpose     : نگهداری اطلاعات کاربران، احراز هویت، تنظیمات شخصی و ارتباطات کاربر با سایر بخش‌های سامانه.
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities
{
    public class ApplicationUser : BaseEntity
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string UserName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string? PhoneNumber { get; set; }

        public string? Avatar { get; set; }

        public string? Bio { get; set; }

        public string? JobTitle { get; set; }

        public bool IsActive { get; set; } = true;

        public bool EmailConfirmed { get; set; } = false;

        public bool PhoneConfirmed { get; set; } = false;

        public DateTime? LastLoginDate { get; set; }

        public LanguageType Language { get; set; } = LanguageType.Persian;

        public ThemeType Theme { get; set; } = ThemeType.Light;

        public string TimeZone { get; set; } = "Asia/Tehran";

        // Navigation Properties
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

        public ICollection<WorkspaceMember> WorkspaceMemberships { get; set; } = new List<WorkspaceMember>();

        public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();

        public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();

        public ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();
    }
}