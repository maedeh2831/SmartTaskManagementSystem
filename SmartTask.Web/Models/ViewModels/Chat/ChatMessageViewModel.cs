using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Chat
{
    public class ChatMessageViewModel
    {
        public int Id { get; set; }

        public int ProjectId { get; set; }

        public int SenderId { get; set; }

        public string SenderName { get; set; } = string.Empty;

        public string? SenderAvatar { get; set; }

        public string Content { get; set; } = string.Empty;

        public ChatMessageType Type { get; set; }

        public string TypeName => Type.ToString();

        public string? AttachmentPath { get; set; }

        public string? AttachmentName { get; set; }

        public long? AttachmentSize { get; set; }

        public int? ReplyToMessageId { get; set; }

        public string? ReplyToSenderName { get; set; }

        public string? ReplyToContent { get; set; }

        public bool IsEdited { get; set; }

        /// <summary>زمان ارسال به‌صورت ISO-8601 با کیفیت UTC تا کلاینت آن را به وقت محلی تبدیل کند.</summary>
        public string CreatedDate { get; set; } = string.Empty;
    }
}
