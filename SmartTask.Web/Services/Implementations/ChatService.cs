using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Common.Extensions;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Chat;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations;

public class ChatService : IChatService
{
    private const int MaxContentLength = 4000;

    private readonly ApplicationDbContext _context;
    private readonly IPresenceTracker _presenceTracker;

    public ChatService(ApplicationDbContext context, IPresenceTracker presenceTracker)
    {
        _context = context;
        _presenceTracker = presenceTracker;
    }

    public async Task<bool> IsMemberAsync(int projectId, int userId)
    {
        return await _context.ProjectMembers
            .AnyAsync(x => x.ProjectId == projectId && x.ApplicationUserId == userId && x.ViewState);
    }

    public async Task<List<int>> GetUserProjectIdsAsync(int userId)
    {
        return await _context.ProjectMembers
            .Where(pm => pm.ApplicationUserId == userId && pm.ViewState)
            .Join(_context.Projects,
                  pm => pm.ProjectId,
                  p => p.Id,
                  (pm, p) => p.Id)
            .Distinct()
            .ToListAsync();
    }

    public async Task<List<ChatListItemViewModel>> GetChatListAsync(int userId)
    {
        var chats = await _context.ProjectMembers
            .Where(pm => pm.ApplicationUserId == userId && pm.ViewState)
            .Join(_context.Projects,
                  pm => pm.ProjectId,
                  p => p.Id,
                  (pm, p) => new ChatListItemViewModel
                  {
                      ProjectId = p.Id,
                      ProjectName = p.Name,
                      ProjectKey = p.Key,
                      Color = p.Color,
                      Icon = p.Icon,
                      MemberCount = p.Members.Count(m => m.ViewState)
                  })
            .ToListAsync();

        if (chats.Count == 0)
            return chats;

        var projectIds = chats.Select(x => x.ProjectId).ToList();

        var unreadCounts = await (
                from m in _context.ChatMessages
                where projectIds.Contains(m.ProjectId)
                      && m.SenderId != userId
                      && m.Id > (_context.ChatReadStates
                            .Where(r => r.ProjectId == m.ProjectId && r.ApplicationUserId == userId)
                            .Select(r => (int?)r.LastReadMessageId)
                            .FirstOrDefault() ?? 0)
                group m by m.ProjectId into g
                select new { ProjectId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ProjectId, x => x.Count);

        var lastMessageIds = await _context.ChatMessages
            .Where(m => projectIds.Contains(m.ProjectId))
            .GroupBy(m => m.ProjectId)
            .Select(g => g.Max(x => x.Id))
            .ToListAsync();

        var lastMessages = await _context.ChatMessages
            .Where(m => lastMessageIds.Contains(m.Id))
            .Include(m => m.Sender)
            .ToDictionaryAsync(m => m.ProjectId);

        foreach (var chat in chats)
        {
            chat.UnreadCount = unreadCounts.TryGetValue(chat.ProjectId, out var count) ? count : 0;

            if (!lastMessages.TryGetValue(chat.ProjectId, out var last))
                continue;

            chat.LastMessageSender = last.SenderId == userId ? "شما" : last.Sender.FullName;
            chat.LastMessagePreview = BuildPreview(last);
            chat.LastMessageDate = ToIso(last.CreatedDate);
            chat.LastActivity = last.CreatedDate;
        }

        return chats
            .OrderByDescending(x => x.LastActivity ?? DateTime.MinValue)
            .ThenBy(x => x.ProjectName)
            .ToList();
    }

    public async Task<ChatRoomViewModel?> GetRoomAsync(int projectId, int userId, int take = 40)
    {
        if (!await IsMemberAsync(projectId, userId))
            return null;

        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == projectId);

        if (project == null)
            return null;

        // یکی بیشتر می‌گیریم تا مشخص شود پیام قدیمی‌تری هم وجود دارد یا نه.
        var rows = await BuildMessageQuery(projectId)
            .OrderByDescending(x => x.Id)
            .Take(take + 1)
            .ToListAsync();

        var hasMore = rows.Count > take;

        if (hasMore)
            rows.RemoveAt(rows.Count - 1);

        rows.Reverse();

        var messages = rows.Select(Map).ToList();

        var role = await _context.ProjectMembers
            .Where(x => x.ProjectId == projectId && x.ApplicationUserId == userId && x.ViewState)
            .Select(x => x.Role)
            .FirstOrDefaultAsync();

        return new ChatRoomViewModel
        {
            ProjectId = project.Id,
            ProjectName = project.Name,
            ProjectKey = project.Key,
            Color = project.Color,
            Icon = project.Icon,
            CanManage = role is ProjectRoleType.Owner or ProjectRoleType.Manager,
            Members = await GetMembersAsync(projectId),
            Messages = messages,
            HasMore = hasMore
        };
    }

    public async Task<List<ChatMessageViewModel>> GetMessagesAsync(int projectId, int? beforeId, int take = 40)
    {
        var query = BuildMessageQuery(projectId);

        if (beforeId.HasValue)
            query = query.Where(x => x.Id < beforeId.Value);

        var rows = await query
            .OrderByDescending(x => x.Id)
            .Take(take)
            .ToListAsync();

        rows.Reverse();

        return rows.Select(Map).ToList();
    }

    public async Task<List<ChatMessageViewModel>> SearchAsync(int projectId, string term, int take = 50)
    {
        if (string.IsNullOrWhiteSpace(term))
            return new List<ChatMessageViewModel>();

        term = term.Trim();

        var rows = await BuildMessageQuery(projectId)
            .Where(x => x.Content.Contains(term))
            .OrderByDescending(x => x.Id)
            .Take(take)
            .ToListAsync();

        return rows.Select(Map).ToList();
    }

    public async Task<ChatMessageViewModel> SendMessageAsync(
        int projectId,
        int senderId,
        string content,
        int? replyToMessageId = null,
        ChatMessageType type = ChatMessageType.Text,
        string? attachmentPath = null,
        string? attachmentName = null,
        long? attachmentSize = null)
    {
        if (!await IsMemberAsync(projectId, senderId))
            throw new InvalidOperationException("شما عضو این پروژه نیستید.");

        content = (content ?? string.Empty).Trim();

        if (content.Length > MaxContentLength)
            content = content[..MaxContentLength];

        if (type == ChatMessageType.Text && string.IsNullOrWhiteSpace(content))
            throw new InvalidOperationException("متن پیام نمی‌تواند خالی باشد.");

        // پاسخ فقط به پیامی از همان گروه مجاز است.
        if (replyToMessageId.HasValue)
        {
            var replyExists = await _context.ChatMessages
                .AnyAsync(x => x.Id == replyToMessageId.Value && x.ProjectId == projectId);

            if (!replyExists)
                replyToMessageId = null;
        }

        var message = new ChatMessage
        {
            ProjectId = projectId,
            SenderId = senderId,
            Content = content,
            Type = type,
            AttachmentPath = attachmentPath,
            AttachmentName = attachmentName,
            AttachmentSize = attachmentSize,
            ReplyToMessageId = replyToMessageId,
            CreatedDate = DateTime.UtcNow,
            ViewState = true
        };

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        // فرستنده پیام خودش را خوانده به حساب می‌آید.
        await MarkAsReadInternalAsync(projectId, senderId, message.Id);

        var row = await BuildMessageQuery(projectId)
            .FirstAsync(x => x.Id == message.Id);

        return Map(row);
    }

    public async Task<ChatMessageViewModel?> EditMessageAsync(int messageId, int userId, string content)
    {
        var message = await _context.ChatMessages
            .FirstOrDefaultAsync(x => x.Id == messageId);

        if (message == null || message.SenderId != userId)
            return null;

        if (message.Type != ChatMessageType.Text)
            return null;

        content = (content ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(content))
            return null;

        if (content.Length > MaxContentLength)
            content = content[..MaxContentLength];

        message.Content = content;
        message.IsEdited = true;
        message.EditedDate = DateTime.UtcNow;
        message.ChangeDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var row = await BuildMessageQuery(message.ProjectId)
            .FirstAsync(x => x.Id == message.Id);

        return Map(row);
    }

    public async Task<int?> DeleteMessageAsync(int messageId, int userId)
    {
        var message = await _context.ChatMessages
            .FirstOrDefaultAsync(x => x.Id == messageId);

        if (message == null)
            return null;

        // فرستنده پیام، یا مالک/مدیر پروژه اجازه حذف دارد.
        if (message.SenderId != userId)
        {
            var role = await _context.ProjectMembers
                .Where(x => x.ProjectId == message.ProjectId && x.ApplicationUserId == userId && x.ViewState)
                .Select(x => (ProjectRoleType?)x.Role)
                .FirstOrDefaultAsync();

            if (role is not (ProjectRoleType.Owner or ProjectRoleType.Manager))
                return null;
        }

        message.ViewState = false;
        message.ChangeDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return message.ProjectId;
    }

    public async Task MarkAsReadAsync(int projectId, int userId)
    {
        if (!await IsMemberAsync(projectId, userId))
            return;

        var lastMessageId = await _context.ChatMessages
            .Where(x => x.ProjectId == projectId)
            .OrderByDescending(x => x.Id)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        if (lastMessageId == 0)
            return;

        await MarkAsReadInternalAsync(projectId, userId, lastMessageId);
    }

    public async Task<int> GetUnreadCountAsync(int projectId, int userId)
    {
        var lastReadId = await _context.ChatReadStates
            .Where(x => x.ProjectId == projectId && x.ApplicationUserId == userId)
            .Select(x => (int?)x.LastReadMessageId)
            .FirstOrDefaultAsync() ?? 0;

        return await _context.ChatMessages
            .CountAsync(x => x.ProjectId == projectId && x.SenderId != userId && x.Id > lastReadId);
    }

    public async Task<int> GetTotalUnreadCountAsync(int userId)
    {
        var projectIds = await GetUserProjectIdsAsync(userId);

        if (projectIds.Count == 0)
            return 0;

        return await _context.ChatMessages
            .CountAsync(m => projectIds.Contains(m.ProjectId)
                             && m.SenderId != userId
                             && m.Id > (_context.ChatReadStates
                                   .Where(r => r.ProjectId == m.ProjectId && r.ApplicationUserId == userId)
                                   .Select(r => (int?)r.LastReadMessageId)
                                   .FirstOrDefault() ?? 0));
    }

    public async Task<List<ChatMemberViewModel>> GetMembersAsync(int projectId)
    {
        var members = await _context.ProjectMembers
            .Where(x => x.ProjectId == projectId && x.ViewState)
            .Include(x => x.ApplicationUser)
            .OrderBy(x => x.Role)
            .Select(x => new ChatMemberViewModel
            {
                UserId = x.ApplicationUserId,
                FullName = x.ApplicationUser.FullName,
                Avatar = x.ApplicationUser.Avatar,
                JobTitle = x.ApplicationUser.JobTitle,
                Role = x.Role,
                WebpushrSubscriberId = x.ApplicationUser.WebpushrSubscriberId
            })
            .ToListAsync();

        foreach (var member in members)
        {
            member.RoleName = member.Role.GetDisplayName();
            member.RoleKey = member.Role.ToString();

            member.IsOnline = _presenceTracker.IsOnline(member.UserId);

            if (!member.IsOnline)
            {
                var lastSeen = _presenceTracker.GetLastSeen(member.UserId);
                member.LastSeen = lastSeen.HasValue ? ToIso(lastSeen.Value) : null;
            }
        }

        return members
            .OrderByDescending(x => x.IsOnline)
            .ThenBy(x => x.Role)
            .ThenBy(x => x.FullName)
            .ToList();
    }

    public async Task<List<int>> GetMemberIdsAsync(int projectId)
    {
        return await _context.ProjectMembers
            .Where(x => x.ProjectId == projectId && x.ViewState)
            .Select(x => x.ApplicationUserId)
            .ToListAsync();
    }

    // ===== Helpers =====

    /// <summary>
    /// پروجکشن خام پیام. قالب‌بندی تاریخ در حافظه انجام می‌شود چون قابل ترجمه به SQL نیست.
    /// </summary>
    private sealed class MessageProjection
    {
        public int Id { get; init; }
        public int ProjectId { get; init; }
        public int SenderId { get; init; }
        public string SenderName { get; init; } = string.Empty;
        public string? SenderAvatar { get; init; }
        public string Content { get; init; } = string.Empty;
        public ChatMessageType Type { get; init; }
        public string? AttachmentPath { get; init; }
        public string? AttachmentName { get; init; }
        public long? AttachmentSize { get; init; }
        public int? ReplyToMessageId { get; init; }
        public string? ReplyToSenderName { get; init; }
        public string? ReplyToContent { get; init; }
        public bool IsEdited { get; init; }
        public DateTime CreatedDate { get; init; }
    }

    private IQueryable<MessageProjection> BuildMessageQuery(int projectId)
    {
        return _context.ChatMessages
            .Where(x => x.ProjectId == projectId)
            .Select(x => new MessageProjection
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                SenderId = x.SenderId,
                SenderName = x.Sender.FullName,
                SenderAvatar = x.Sender.Avatar,
                Content = x.Content,
                Type = x.Type,
                AttachmentPath = x.AttachmentPath,
                AttachmentName = x.AttachmentName,
                AttachmentSize = x.AttachmentSize,
                ReplyToMessageId = x.ReplyToMessageId,
                ReplyToSenderName = x.ReplyToMessage != null ? x.ReplyToMessage.Sender.FullName : null,
                ReplyToContent = x.ReplyToMessage != null
                    ? (x.ReplyToMessage.Type == ChatMessageType.Text
                        ? x.ReplyToMessage.Content
                        : x.ReplyToMessage.AttachmentName)
                    : null,
                IsEdited = x.IsEdited,
                CreatedDate = x.CreatedDate
            });
    }

    private static ChatMessageViewModel Map(MessageProjection x) => new()
    {
        Id = x.Id,
        ProjectId = x.ProjectId,
        SenderId = x.SenderId,
        SenderName = x.SenderName,
        SenderAvatar = x.SenderAvatar,
        Content = x.Content,
        Type = x.Type,
        AttachmentPath = x.AttachmentPath,
        AttachmentName = x.AttachmentName,
        AttachmentSize = x.AttachmentSize,
        ReplyToMessageId = x.ReplyToMessageId,
        ReplyToSenderName = x.ReplyToSenderName,
        ReplyToContent = x.ReplyToContent,
        IsEdited = x.IsEdited,
        CreatedDate = ToIso(x.CreatedDate)
    };

    private async Task MarkAsReadInternalAsync(int projectId, int userId, int lastMessageId)
    {
        var state = await _context.ChatReadStates
            .FirstOrDefaultAsync(x => x.ProjectId == projectId && x.ApplicationUserId == userId);

        if (state == null)
        {
            _context.ChatReadStates.Add(new ChatReadState
            {
                ProjectId = projectId,
                ApplicationUserId = userId,
                LastReadMessageId = lastMessageId,
                LastReadDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                ViewState = true
            });
        }
        else
        {
            if (state.LastReadMessageId >= lastMessageId)
                return;

            state.LastReadMessageId = lastMessageId;
            state.LastReadDate = DateTime.UtcNow;
            state.ChangeDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    private static string BuildPreview(ChatMessage message)
    {
        return message.Type switch
        {
            ChatMessageType.Image => "🖼 تصویر",
            ChatMessageType.File => $"📎 {message.AttachmentName}",
            _ => message.Content.Length > 60 ? message.Content[..60] + "…" : message.Content
        };
    }

    private static string ToIso(DateTime value)
        => value.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
}
