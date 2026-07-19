/*
| Module      : Identity
| Entity      : UserRole
| Purpose     : مدیریت ارتباط چند به چند بین کاربران و نقش‌های سیستم.
*/

namespace SmartTask.Web.Models.Entities
{
    public class UserRole : BaseEntity
    {
        // Properties
        public int ApplicationUserId { get; set; }

        public int RoleId { get; set; }

        // Navigation Properties
        public ApplicationUser ApplicationUser { get; set; } = null!;

        public Role Role { get; set; } = null!;
    }
}