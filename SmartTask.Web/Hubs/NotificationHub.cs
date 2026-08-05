using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SmartTask.Web.Infrastructure.Interfaces;

namespace SmartTask.Web.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ICurrentUserService _currentUser;

        public NotificationHub(ICurrentUserService currentUser)
        {
            _currentUser = currentUser;
        }

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetUserGroupName(_currentUser.UserId));
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetUserGroupName(_currentUser.UserId));
            await base.OnDisconnectedAsync(exception);
        }

        public static string GetUserGroupName(int userId) => $"user-{userId}";
    }
}