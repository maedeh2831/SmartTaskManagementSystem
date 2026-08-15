using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Services.Interfaces;
using System.Security.Claims;

namespace SmartTask.Web.Hubs
{
    /// <summary>
    /// هاب گفتگوی گروهی پروژه‌ها؛ ارسال بلادرنگ پیام و اعلام وضعیت آنلاین/آفلاین اعضا.
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IPresenceTracker _presenceTracker;

        public ChatHub(IChatService chatService, IPresenceTracker presenceTracker)
        {
            _chatService = chatService;
            _presenceTracker = presenceTracker;
        }

        public static string GetProjectGroupName(int projectId) => $"project-chat-{projectId}";

        private int UserId
        {
            get
            {
                var value = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return int.TryParse(value, out var id) ? id : 0;
            }
        }

        private string UserName => Context.User?.Identity?.Name ?? string.Empty;

        public override async Task OnConnectedAsync()
        {
            var userId = UserId;

            if (userId == 0)
            {
                Context.Abort();
                return;
            }

            var projectIds = await _chatService.GetUserProjectIdsAsync(userId);

            foreach (var projectId in projectIds)
                await Groups.AddToGroupAsync(Context.ConnectionId, GetProjectGroupName(projectId));

            var becameOnline = _presenceTracker.Connect(userId, Context.ConnectionId);

            if (becameOnline)
            {
                foreach (var projectId in projectIds)
                {
                    await Clients.OthersInGroup(GetProjectGroupName(projectId))
                        .SendAsync("UserOnline", new { projectId, userId });
                }
            }

            // وضعیت فعلی سایر کاربران آنلاین برای همین اتصال.
            await Clients.Caller.SendAsync("OnlineUsers", _presenceTracker.GetOnlineUsers());

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = UserId;

            if (userId != 0)
            {
                var wentOffline = _presenceTracker.Disconnect(userId, Context.ConnectionId);

                if (wentOffline)
                {
                    var projectIds = await _chatService.GetUserProjectIdsAsync(userId);
                    var lastSeen = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

                    foreach (var projectId in projectIds)
                    {
                        await Clients.Group(GetProjectGroupName(projectId))
                            .SendAsync("UserOffline", new { projectId, userId, lastSeen });
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>ارسال پیام به گروه پروژه.</summary>
        public async Task SendMessage(int projectId, string content, int? replyToMessageId)
        {
            var userId = UserId;

            if (!await _chatService.IsMemberAsync(projectId, userId))
                throw new HubException("شما عضو این پروژه نیستید.");

            try
            {
                var message = await _chatService.SendMessageAsync(projectId, userId, content, replyToMessageId);

                await Clients.Group(GetProjectGroupName(projectId))
                    .SendAsync("ReceiveMessage", message);
            }
            catch (InvalidOperationException ex)
            {
                throw new HubException(ex.Message);
            }
        }

        public async Task EditMessage(int messageId, string content)
        {
            var message = await _chatService.EditMessageAsync(messageId, UserId, content);

            if (message == null)
                throw new HubException("ویرایش این پیام امکان‌پذیر نیست.");

            await Clients.Group(GetProjectGroupName(message.ProjectId))
                .SendAsync("MessageEdited", message);
        }

        public async Task DeleteMessage(int messageId)
        {
            var projectId = await _chatService.DeleteMessageAsync(messageId, UserId);

            if (projectId == null)
                throw new HubException("حذف این پیام امکان‌پذیر نیست.");

            await Clients.Group(GetProjectGroupName(projectId.Value))
                .SendAsync("MessageDeleted", new { projectId = projectId.Value, messageId });
        }

        /// <summary>اعلام «در حال نوشتن» به سایر اعضای گروه.</summary>
        public async Task Typing(int projectId, bool isTyping)
        {
            var userId = UserId;

            if (!await _chatService.IsMemberAsync(projectId, userId))
                return;

            await Clients.OthersInGroup(GetProjectGroupName(projectId))
                .SendAsync("UserTyping", new { projectId, userId, userName = UserName, isTyping });
        }

        /// <summary>علامت‌گذاری پیام‌های گروه به‌عنوان خوانده‌شده.</summary>
        public async Task MarkAsRead(int projectId)
        {
            await _chatService.MarkAsReadAsync(projectId, UserId);
        }

        /// <summary>عضویت در گروه یک پروژه (پس از ساخت پروژه یا افزوده‌شدن کاربر بدون بارگذاری مجدد صفحه).</summary>
        public async Task JoinProject(int projectId)
        {
            if (!await _chatService.IsMemberAsync(projectId, UserId))
                throw new HubException("شما عضو این پروژه نیستید.");

            await Groups.AddToGroupAsync(Context.ConnectionId, GetProjectGroupName(projectId));
        }
    }
}
