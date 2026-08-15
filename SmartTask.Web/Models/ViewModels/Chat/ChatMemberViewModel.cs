using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Chat
{
    public class ChatMemberViewModel
    {
        public int UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string? Avatar { get; set; }

        public string? JobTitle { get; set; }

        public ProjectRoleType Role { get; set; }

        /// <summary>نام نمایشی فارسی نقش؛ چون enum در JSON به‌صورت عدد سریالایز می‌شود.</summary>
        public string RoleName { get; set; } = string.Empty;

        /// <summary>نام انگلیسی نقش برای ساخت کلاس CSS.</summary>
        public string RoleKey { get; set; } = string.Empty;

        public bool IsOnline { get; set; }

        /// <summary>آخرین حضور به‌صورت ISO-8601 (UTC). در صورت آنلاین بودن یا نامشخص بودن، null است.</summary>
        public string? LastSeen { get; set; }
    }
}
