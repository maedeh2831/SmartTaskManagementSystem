/*
| Module      : Chat
| Entity      : ChatMessage
| Purpose     : پیام‌های گفتگوی گروهی هر پروژه.
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities
{
    public class ChatMessage : BaseEntity
    {
        // Properties

        public int ProjectId { get; set; }

        public int SenderId { get; set; }

        public string Content { get; set; } = string.Empty;

        public ChatMessageType Type { get; set; } = ChatMessageType.Text;

        public string? AttachmentPath { get; set; }

        public string? AttachmentName { get; set; }

        public long? AttachmentSize { get; set; }

        public int? ReplyToMessageId { get; set; }

        public bool IsEdited { get; set; }

        public DateTime? EditedDate { get; set; }

        public bool IsPinned { get; set; }

        public DateTime? PinnedDate { get; set; }

        // Navigation Properties

        public Project Project { get; set; } = null!;

        public ApplicationUser Sender { get; set; } = null!;

        public ChatMessage? ReplyToMessage { get; set; }

        public ICollection<ChatMessage> Replies { get; set; } = new List<ChatMessage>();
    }
}
