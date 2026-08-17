using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Services.Interfaces;
using System.Security.Claims;

namespace SmartTask.Web.Hubs
{
    /// <summary>
    /// هاب گفتگوی گروهی پروژه‌ها؛ ارسال بلادرنگ پیام،
    /// وضعیت آنلاین/آفلاین و ارسال Push Notification.
    /// </summary>
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IPresenceTracker _presenceTracker;
        private readonly IWebpushrService _webpushrService;

        public ChatHub(
            IChatService chatService,
            IPresenceTracker presenceTracker,
            IWebpushrService webpushrService)
        {
            _chatService = chatService;
            _presenceTracker = presenceTracker;
            _webpushrService = webpushrService;
        }

        public static string GetProjectGroupName(int projectId)
            => $"project-chat-{projectId}";

        private int UserId
        {
            get
            {
                var value = Context.User?.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                return int.TryParse(value, out var id) ? id : 0;
            }
        }

        private string UserName
            => Context.User?.Identity?.Name ?? string.Empty;


        // =========================================================
        // CONNECTION
        // =========================================================

        public override async Task OnConnectedAsync()
        {
            var userId = UserId;

            if (userId == 0)
            {
                Context.Abort();
                return;
            }

            var projectIds =
                await _chatService.GetUserProjectIdsAsync(userId);

            foreach (var projectId in projectIds)
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    GetProjectGroupName(projectId));
            }

            var becameOnline =
                _presenceTracker.Connect(
                    userId,
                    Context.ConnectionId);

            if (becameOnline)
            {
                foreach (var projectId in projectIds)
                {
                    await Clients.OthersInGroup(
                            GetProjectGroupName(projectId))
                        .SendAsync(
                            "UserOnline",
                            new
                            {
                                projectId,
                                userId
                            });
                }
            }

            await Clients.Caller.SendAsync(
                "OnlineUsers",
                _presenceTracker.GetOnlineUsers());

            await base.OnConnectedAsync();
        }


        public override async Task OnDisconnectedAsync(
            Exception? exception)
        {
            var userId = UserId;

            if (userId != 0)
            {
                var wentOffline =
                    _presenceTracker.Disconnect(
                        userId,
                        Context.ConnectionId);

                if (wentOffline)
                {
                    var projectIds =
                        await _chatService.GetUserProjectIdsAsync(
                            userId);

                    var lastSeen =
                        DateTime.UtcNow.ToString(
                            "yyyy-MM-ddTHH:mm:ss.fffZ");

                    foreach (var projectId in projectIds)
                    {
                        await Clients.Group(
                                GetProjectGroupName(projectId))
                            .SendAsync(
                                "UserOffline",
                                new
                                {
                                    projectId,
                                    userId,
                                    lastSeen
                                });
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }


        // =========================================================
        // SEND MESSAGE
        // =========================================================

public async Task SendMessage(
    int projectId,
    string content,
    int? replyToMessageId)
{
    var userId = UserId;

    if (userId == 0)
        throw new HubException("کاربر شناسایی نشد.");

    if (!await _chatService.IsMemberAsync(projectId, userId))
        throw new HubException(
            "شما عضو این پروژه نیستید.");

    try
    {
        // -------------------------------------------------
        // Save message
        // -------------------------------------------------

        var message =
            await _chatService.SendMessageAsync(
                projectId,
                userId,
                content,
                replyToMessageId);

        // -------------------------------------------------
        // SignalR - Live message
        // -------------------------------------------------

        await Clients
            .Group(GetProjectGroupName(projectId))
            .SendAsync(
                "ReceiveMessage",
                message);

        // -------------------------------------------------
        // Webpushr - Push notification to other members
        // -------------------------------------------------

        await _webpushrService.SendChatMessagePushAsync(
            projectId,
            userId,
            message.SenderName,
            message.Content);
    }
    catch (InvalidOperationException ex)
    {
        throw new HubException(ex.Message);
    }
}


        // =========================================================
        // EDIT MESSAGE
        // =========================================================

        public async Task EditMessage(
            int messageId,
            string content)
        {
            var message =
                await _chatService.EditMessageAsync(
                    messageId,
                    UserId,
                    content);

            if (message == null)
            {
                throw new HubException(
                    "ویرایش این پیام امکان‌پذیر نیست.");
            }

            await Clients.Group(
                    GetProjectGroupName(message.ProjectId))
                .SendAsync(
                    "MessageEdited",
                    message);
        }


        // =========================================================
        // DELETE MESSAGE
        // =========================================================

        public async Task DeleteMessage(
            int messageId)
        {
            var projectId =
                await _chatService.DeleteMessageAsync(
                    messageId,
                    UserId);

            if (projectId == null)
            {
                throw new HubException(
                    "حذف این پیام امکان‌پذیر نیست.");
            }

            await Clients.Group(
                    GetProjectGroupName(projectId.Value))
                .SendAsync(
                    "MessageDeleted",
                    new
                    {
                        projectId = projectId.Value,
                        messageId
                    });
        }


        // =========================================================
        // TYPING
        // =========================================================

        public async Task Typing(
            int projectId,
            bool isTyping)
        {
            var userId = UserId;

            if (!await _chatService.IsMemberAsync(
                    projectId,
                    userId))
                return;

            await Clients.OthersInGroup(
                    GetProjectGroupName(projectId))
                .SendAsync(
                    "UserTyping",
                    new
                    {
                        projectId,
                        userId,
                        userName = UserName,
                        isTyping
                    });
        }


        // =========================================================
        // READ
        // =========================================================

        public async Task MarkAsRead(
            int projectId)
        {
            await _chatService.MarkAsReadAsync(
                projectId,
                UserId);
        }


        // =========================================================
        // TEST PUSH
        // =========================================================

        public async Task TestPush(
            int projectId)
        {
            var userId = UserId;

            if (userId == 0)
                throw new HubException("کاربر شناسایی نشد.");

            if (!await _chatService.IsMemberAsync(
                    projectId,
                    userId))
            {
                throw new HubException(
                    "شما عضو این پروژه نیستید.");
            }

            await _webpushrService.SendTestPushAsync(
                projectId,
                userId,
                UserName);
        }


        // =========================================================
        // JOIN PROJECT
        // =========================================================

        public async Task JoinProject(
            int projectId)
        {
            if (!await _chatService.IsMemberAsync(
                    projectId,
                    UserId))
            {
                throw new HubException(
                    "شما عضو این پروژه نیستید.");
            }

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                GetProjectGroupName(projectId));
        }

    }
}