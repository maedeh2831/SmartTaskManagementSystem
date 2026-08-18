namespace SmartTask.Web.Models.ViewModels.Chat
{
    public class ChatIndexViewModel
    {
        public int CurrentUserId { get; set; }

        public string CurrentUserName { get; set; } = string.Empty;

        public bool IsJalali { get; set; } = true;

        /// <summary>لیست گروه‌های گفتگو (پروژه‌هایی که کاربر عضو آن‌هاست).</summary>
        public List<ChatListItemViewModel> Chats { get; set; } = new();

        /// <summary>گفتگوی باز شده. در صورتی که کاربر عضو هیچ پروژه‌ای نباشد null است.</summary>
        public ChatRoomViewModel? ActiveRoom { get; set; }
    }
}
