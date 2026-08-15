namespace SmartTask.Web.Models.ViewModels.Chat
{
    /// <summary>یک ردیف در لیست گفتگوها (هر پروژه = یک گروه).</summary>
    public class ChatListItemViewModel
    {
        public int ProjectId { get; set; }

        public string ProjectName { get; set; } = string.Empty;

        public string ProjectKey { get; set; } = string.Empty;

        public string? Color { get; set; }

        public string? Icon { get; set; }

        public int MemberCount { get; set; }

        public int UnreadCount { get; set; }

        public string? LastMessageSender { get; set; }

        public string? LastMessagePreview { get; set; }

        public string? LastMessageDate { get; set; }

        public DateTime? LastActivity { get; set; }
    }
}
