namespace SmartTask.Web.Models.ViewModels.Chat
{
    /// <summary>اتاق گفتگوی یک پروژه به‌همراه پیام‌ها و اعضا.</summary>
    public class ChatRoomViewModel
    {
        public int ProjectId { get; set; }

        public string ProjectName { get; set; } = string.Empty;

        public string ProjectKey { get; set; } = string.Empty;

        public string? Color { get; set; }

        public string? Icon { get; set; }

        public bool CanManage { get; set; }

        public List<ChatMemberViewModel> Members { get; set; } = new();

        /// <summary>پیام‌ها به ترتیب قدیمی به جدید.</summary>
        public List<ChatMessageViewModel> Messages { get; set; } = new();

        /// <summary>آیا پیام قدیمی‌تری برای بارگذاری وجود دارد.</summary>
        public bool HasMore { get; set; }

        public int OnlineCount => Members.Count(x => x.IsOnline);
    }
}
