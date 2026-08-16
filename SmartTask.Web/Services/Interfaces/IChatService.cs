using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Chat;

namespace SmartTask.Web.Services.Interfaces;

public interface IChatService
{
    /// <summary>بررسی عضویت کاربر در پروژه (مجوز دسترسی به گفتگوی گروه).</summary>
    Task<bool> IsMemberAsync(int projectId, int userId);

    /// <summary>شناسه پروژه‌هایی که کاربر در آن‌ها عضو است؛ برای عضویت در گروه‌های SignalR.</summary>
    Task<List<int>> GetUserProjectIdsAsync(int userId);

    /// <summary>لیست گفتگوها (هر پروژه یک گروه) به‌همراه آخرین پیام و تعداد خوانده‌نشده.</summary>
    Task<List<ChatListItemViewModel>> GetChatListAsync(int userId);

    Task<ChatRoomViewModel?> GetRoomAsync(int projectId, int userId, int take = 40);

    /// <summary>پیام‌های قدیمی‌تر از <paramref name="beforeId"/> برای پیمایش تاریخچه.</summary>
    Task<List<ChatMessageViewModel>> GetMessagesAsync(int projectId, int? beforeId, int take = 40);

    Task<List<ChatMessageViewModel>> SearchAsync(int projectId, string term, int take = 50);

    Task<ChatMessageViewModel> SendMessageAsync(
        int projectId,
        int senderId,
        string content,
        int? replyToMessageId = null,
        ChatMessageType type = ChatMessageType.Text,
        string? attachmentPath = null,
        string? attachmentName = null,
        long? attachmentSize = null);

    Task<ChatMessageViewModel?> EditMessageAsync(int messageId, int userId, string content);

    /// <summary>حذف نرم پیام. در صورت موفقیت شناسه پروژه را برمی‌گرداند.</summary>
    Task<int?> DeleteMessageAsync(int messageId, int userId);

    Task MarkAsReadAsync(int projectId, int userId);

    Task<int> GetUnreadCountAsync(int projectId, int userId);

    Task<int> GetTotalUnreadCountAsync(int userId);

    Task<List<ChatMemberViewModel>> GetMembersAsync(int projectId);

    /// <summary>شناسه اعضای پروژه؛ برای اطلاع‌رسانی وضعیت آنلاین/آفلاین.</summary>
    Task<List<int>> GetMemberIdsAsync(int projectId);
}
