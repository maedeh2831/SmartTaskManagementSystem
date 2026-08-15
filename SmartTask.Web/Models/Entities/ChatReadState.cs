/*
| Module      : Chat
| Entity      : ChatReadState
| Purpose     : نگهداری آخرین پیام خوانده‌شده هر کاربر در گفتگوی هر پروژه (برای شمارش پیام‌های خوانده‌نشده).
*/

namespace SmartTask.Web.Models.Entities
{
    public class ChatReadState : BaseEntity
    {
        // Properties

        public int ProjectId { get; set; }

        public int ApplicationUserId { get; set; }

        public int LastReadMessageId { get; set; }

        public DateTime LastReadDate { get; set; } = DateTime.UtcNow;

        // Navigation Properties

        public Project Project { get; set; } = null!;

        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}
