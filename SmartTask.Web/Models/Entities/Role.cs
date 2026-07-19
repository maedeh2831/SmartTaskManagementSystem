/*
| Module      : Identity
| Entity      : Role
| Purpose     : تعریف نقش‌های سیستم و مدیریت سطح دسترسی کاربران.
*/

namespace SmartTask.Web.Models.Entities
{
    public class Role : BaseEntity
    {
        // Properties
        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        // Navigation Properties
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}