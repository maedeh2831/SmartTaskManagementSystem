using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Common.Extensions;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Chat;
using SmartTask.Web.Services.Files;
using SmartTask.Web.Services.Interfaces;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace SmartTask.Web.Services.Implementations;

public class ChatService : IChatService
{
    private const int MaxContentLength = 4000;
    private const int RateLimitWindowSeconds = 3;
    private const int RateLimitMaxMessages = 5;

    private readonly ApplicationDbContext _context;
    private readonly IPresenceTracker _presenceTracker;

    // Simple in-memory rate limiter: userId -> list of timestamps
    private static readonly ConcurrentDictionary<int, List<DateTime>> _sendTimestamps = new();

    // Mention pattern: @name (Persian/English names, no space before)
    private static readonly Regex MentionPattern = new(
        @"@([\p{L}\p{N}_]+)",
        RegexOptions.Compiled);

    public ChatService(ApplicationDbContext context, IPresenceTracker presenceTracker)
    {
        _context = context;
        _presenceTracker = presenceTracker;
    }

    // =========================================================
    // MEMBERSHIP
    // =========================================================

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

    // =========================================================
    // CHAT LIST
    // =========================================================

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

    // =========================================================
    // ROOM
    // =========================================================

    public async Task<ChatRoomViewModel?> GetRoomAsync(int projectId, int userId, int take = 40)
    {
        if (!await IsMemberAsync(projectId, userId))
            return null;

        var project = await _context.Projects
            .FirstOrDefaultAsync(x => x.Id == projectId);

        if (project == null)
            return null;

        var rows = await BuildMessageQuery(projectId)
            .OrderByDescending(x => x.Id)
            .Take(take + 1)
            .ToListAsync();

        var hasMore = rows.Count > take;

        if (hasMore)
            rows.RemoveAt(rows.Count - 1);

        rows.Reverse();

        var messages = rows.Select(Map).ToList();

        // Load reactions for all messages
        var messageIds = messages.Select(m => m.Id).ToList();
        var reactions = await LoadReactionsAsync(messageIds, userId);
        ApplyReactions(messages, reactions);

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

    // =========================================================
    // MESSAGES
    // =========================================================

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

        var messages = rows.Select(Map).ToList();

        var messageIds = messages.Select(m => m.Id).ToList();
        var reactions = await LoadReactionsAsync(messageIds, userId: 0);
        ApplyReactions(messages, reactions);

        return messages;
    }

    // =========================================================
    // SEARCH (with pagination)
    // =========================================================

    public async Task<List<ChatMessageViewModel>> SearchAsync(int projectId, string term, int take = 50, int skip = 0)
    {
        if (string.IsNullOrWhiteSpace(term))
            return new List<ChatMessageViewModel>();

        term = term.Trim();

        var rows = await BuildMessageQuery(projectId)
            .Where(x => x.Content.Contains(term))
            .OrderByDescending(x => x.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        var messages = rows.Select(Map).ToList();

        var messageIds = messages.Select(m => m.Id).ToList();
        var reactions = await LoadReactionsAsync(messageIds, userId: 0);
        ApplyReactions(messages, reactions);

        return messages;
    }

    // =========================================================
    // SEND MESSAGE
    // =========================================================

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

        // Parse mentions from content
        var mentionedUserIds = await ParseMentions(content, projectId);

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

        var viewModel = Map(row);
        viewModel.MentionedUserIds = mentionedUserIds;

        return viewModel;
    }

    // =========================================================
    // EDIT MESSAGE
    // =========================================================

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

    // =========================================================
    // DELETE MESSAGE (with physical file deletion)
    // =========================================================

    public async Task<int?> DeleteMessageAsync(int messageId, int userId, IFileUploadService? fileUploadService = null)
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

        // Delete physical attachment file from storage
        if (fileUploadService != null &&
            !string.IsNullOrWhiteSpace(message.AttachmentPath))
        {
            fileUploadService.DeleteFile(message.AttachmentPath);
        }

        message.ViewState = false;
        message.ChangeDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return message.ProjectId;
    }

    // =========================================================
    // MARK AS READ
    // =========================================================

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

    // =========================================================
    // UNREAD COUNTS
    // =========================================================

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

    // =========================================================
    // MEMBERS
    // =========================================================

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

    // =========================================================
    // REACTIONS
    // =========================================================

    public async Task<ChatMessageViewModel?> ToggleReactionAsync(int messageId, int userId, string emoji)
    {
        var message = await _context.ChatMessages
            .FirstOrDefaultAsync(x => x.Id == messageId);

        if (message == null)
            return null;

        // Check membership
        if (!await IsMemberAsync(message.ProjectId, userId))
            return null;

        var existing = await _context.ChatMessageReactions
            .FirstOrDefaultAsync(x => x.ChatMessageId == messageId && x.UserId == userId);

        if (existing != null)
        {
            if (existing.Emoji == emoji)
            {
                // Remove existing reaction (toggle off)
                _context.ChatMessageReactions.Remove(existing);
            }
            else
            {
                // Change reaction
                existing.Emoji = emoji;
                existing.ChangeDate = DateTime.UtcNow;
            }
        }
        else
        {
            // Add new reaction
            _context.ChatMessageReactions.Add(new ChatMessageReaction
            {
                ChatMessageId = messageId,
                UserId = userId,
                Emoji = emoji,
                CreatedDate = DateTime.UtcNow,
                ViewState = true
            });
        }

        await _context.SaveChangesAsync();

        return await BuildMessageWithReactionsAsync(message.ProjectId, messageId, userId);
    }

    // =========================================================
    // PIN
    // =========================================================

    public async Task<ChatMessageViewModel?> TogglePinAsync(int messageId, int userId)
    {
        var message = await _context.ChatMessages
            .FirstOrDefaultAsync(x => x.Id == messageId);

        if (message == null)
            return null;

        // Only owner/manager can pin
        var role = await _context.ProjectMembers
            .Where(x => x.ProjectId == message.ProjectId && x.ApplicationUserId == userId && x.ViewState)
            .Select(x => (ProjectRoleType?)x.Role)
            .FirstOrDefaultAsync();

        if (role is not (ProjectRoleType.Owner or ProjectRoleType.Manager))
            return null;

        message.IsPinned = !message.IsPinned;
        message.PinnedDate = message.IsPinned ? DateTime.UtcNow : null;
        message.ChangeDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await BuildMessageWithReactionsAsync(message.ProjectId, messageId, userId);
    }

    public async Task<List<ChatMessageViewModel>> GetPinnedMessagesAsync(int projectId)
    {
        var rows = await BuildMessageQuery(projectId)
            .Where(x => x.IsPinned)
            .OrderByDescending(x => x.PinnedDate)
            .ToListAsync();

        var messages = rows.Select(Map).ToList();

        var messageIds = messages.Select(m => m.Id).ToList();
        var reactions = await LoadReactionsAsync(messageIds, userId: 0);
        ApplyReactions(messages, reactions);

        return messages;
    }

    // =========================================================
    // RATE LIMITING
    // =========================================================

    public Task<bool> CanSendMessageAsync(int projectId, int userId)
    {
        var now = DateTime.UtcNow;
        var cutoff = now.AddSeconds(-RateLimitWindowSeconds);

        var timestamps = _sendTimestamps.GetOrAdd(userId, _ => new List<DateTime>());

        lock (timestamps)
        {
            // Remove old timestamps
            timestamps.RemoveAll(t => t < cutoff);

            if (timestamps.Count >= RateLimitMaxMessages)
                return Task.FromResult(false);

            timestamps.Add(now);
            return Task.FromResult(true);
        }
    }

    // =========================================================
    // HELPERS
    // =========================================================

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
        public bool IsPinned { get; init; }
        public DateTime? PinnedDate { get; init; }
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
                IsPinned = x.IsPinned,
                PinnedDate = x.PinnedDate,
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
        IsPinned = x.IsPinned,
        PinnedDate = x.PinnedDate.HasValue ? ToIso(x.PinnedDate.Value) : null,
        CreatedDate = ToIso(x.CreatedDate)
    };

    private async Task<ChatMessageViewModel?> BuildMessageWithReactionsAsync(int projectId, int messageId, int userId)
    {
        var row = await BuildMessageQuery(projectId)
            .FirstOrDefaultAsync(x => x.Id == messageId);

        if (row == null)
            return null;

        var message = Map(row);
        var reactions = await LoadReactionsAsync(new List<int> { messageId }, userId);
        ApplyReactions(new List<ChatMessageViewModel> { message }, reactions);

        return message;
    }

    private async Task<Dictionary<int, List<ChatMessageReaction>>> LoadReactionsAsync(List<int> messageIds, int userId)
    {
        if (messageIds.Count == 0)
            return new Dictionary<int, List<ChatMessageReaction>>();

        var reactions = await _context.ChatMessageReactions
            .Where(x => messageIds.Contains(x.ChatMessageId) && x.ViewState)
            .ToListAsync();

        return reactions
            .GroupBy(x => x.ChatMessageId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    private static void ApplyReactions(
        List<ChatMessageViewModel> messages,
        Dictionary<int, List<ChatMessageReaction>> reactions)
    {
        foreach (var msg in messages)
        {
            if (!reactions.TryGetValue(msg.Id, out var msgReactions))
                continue;

            msg.Reactions = msgReactions
                .GroupBy(r => r.Emoji)
                .Select(g => new ChatReactionViewModel
                {
                    Emoji = g.Key,
                    Count = g.Count(),
                    UserIds = g.Select(r => r.UserId).ToList()
                })
                .OrderByDescending(x => x.Count)
                .ToList();
        }
    }

    /// <summary>
    /// Parse @mentions from message content and return mentioned user IDs.
    /// </summary>
    private async Task<List<int>> ParseMentions(string content, int projectId)
    {
        if (string.IsNullOrWhiteSpace(content))
            return new List<int>();

        var matches = MentionPattern.Matches(content);
        if (matches.Count == 0)
            return new List<int>();

        var mentionNames = matches
            .Select(m => m.Groups[1].Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Get all project members
        var members = await _context.ProjectMembers
            .Where(x => x.ProjectId == projectId && x.ViewState)
            .Include(x => x.ApplicationUser)
            .Select(x => new { x.ApplicationUserId, x.ApplicationUser.FullName })
            .ToListAsync();

        var mentionedIds = new List<int>();

        foreach (var name in mentionNames)
        {
            var member = members.FirstOrDefault(m =>
                m.FullName.Replace(" ", "").Contains(name, StringComparison.OrdinalIgnoreCase) ||
                m.FullName.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (member != null)
                mentionedIds.Add(member.ApplicationUserId);
        }

        return mentionedIds;
    }

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
