/*
| Module      : Identity
| Entity      : ApplicationUser
| Purpose     : نگهداری اطلاعات کاربران، احراز هویت، تنظیمات شخصی و ارتباطات کاربر با سایر بخش‌های سامانه.
*/

using Microsoft.AspNetCore.Identity;
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string FullName
        {
            get
            {
                return $"{FirstName} {LastName}".Trim();
            }
        }

        public string? Avatar { get; set; }

        public string? Bio { get; set; }

        public string? JobTitle { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? LastLoginDate { get; set; }

        public LanguageType Language { get; set; } = LanguageType.Persian;

        public ThemeType Theme { get; set; } = ThemeType.Light;

        public string TimeZone { get; set; } = "Asia/Tehran";

        public string? ChangeUser { get; set; }

        public DateTime? ChangeDate { get; set; }

        public bool ViewState { get; set; } = true;

        // Navigation Properties

        public ICollection<WorkspaceMember> WorkspaceMemberships { get; set; } = new List<WorkspaceMember>();

        public ICollection<TeamMember> TeamMemberships { get; set; } = new List<TeamMember>();

        public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();

        public ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

        public virtual ICollection<Reminder> Reminders { get; set; } = new HashSet<Reminder>();

        public ICollection<Notification> Notifications { get; set; } = new HashSet<Notification>();

        public ICollection<ActivityLog> ActivityLogs { get; set; } = new HashSet<ActivityLog>();

        public virtual ICollection<TimeLog> TimeLogs { get; set; } = new HashSet<TimeLog>();
    }
}