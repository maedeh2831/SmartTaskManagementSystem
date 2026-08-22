/*
| Module      : Chat
| Entity      : ChatMessageReaction
| Purpose     : واکنش‌های ایموجی روی پیام‌های گفتگوی گروهی.
*/

namespace SmartTask.Web.Models.Entities
{
    public class ChatMessageReaction : BaseEntity
    {
        // Properties

        public int ChatMessageId { get; set; }

        public int UserId { get; set; }

        public string Emoji { get; set; } = string.Empty;

        // Navigation Properties

        public ChatMessage ChatMessage { get; set; } = null!;

        public ApplicationUser User { get; set; } = null!;
    }
}
