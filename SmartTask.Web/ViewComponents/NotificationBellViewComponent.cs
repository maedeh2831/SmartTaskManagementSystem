using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.ViewModels.Notification;
using SmartTask.Web.Models.ViewModels.Shared;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.ViewComponents
{
    public class NotificationBellViewComponent : ViewComponent
    {
        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUser;

        public NotificationBellViewComponent(
            INotificationService notificationService,
            ICurrentUserService currentUser)
        {
            _notificationService = notificationService;
            _currentUser = currentUser;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!_currentUser.IsAuthenticated)
                return View(new NotificationBellViewModel());

            var recent = await _notificationService.GetRecentAsync(_currentUser.UserId, 8);
            var unreadCount = await _notificationService.GetUnreadCountAsync(_currentUser.UserId);

            var model = new NotificationBellViewModel
            {
                UnreadCount = unreadCount,
                RecentNotifications = recent.Select(x => new NotificationItemViewModel
                {
                    Id = x.Id,
                    Title = x.Title,
                    Message = x.Message,
                    Type = x.Type,
                    IsRead = x.IsRead,
                    CreateDate = x.CreatedDate
                }).ToList()
            };

            return View(model);
        }
    }
}