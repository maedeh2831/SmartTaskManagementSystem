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

        public bool IsPinned { get; set; }

        public string? PinnedDate { get; set; }

        /// <summary>لیست واکنش‌های ایموجی این پیام.</summary>
        public List<ChatReactionViewModel> Reactions { get; set; } = new();

        /// <summary>شناسه کاربرانی که در متن پیام mention شده‌اند.</summary>
        public List<int> MentionedUserIds { get; set; } = new();

        /// <summary>زمان ارسال به‌صورت ISO-8601 با کیفیت UTC تا کلاینت آن را به وقت محلی تبدیل کند.</summary>
        public string CreatedDate { get; set; } = string.Empty;
    }

    public class ChatReactionViewModel
    {
        public string Emoji { get; set; } = string.Empty;

        public int Count { get; set; }

        /// <summary>آیا کاربر جاری این واکنش را انتخاب کرده؟</summary>
        public bool HasReacted { get; set; }

        /// <summary>شناسه کاربرانی که این واکنش را زده‌اند (برای نمایش tooltip).</summary>
        public List<int> UserIds { get; set; } = new();
    }
}
