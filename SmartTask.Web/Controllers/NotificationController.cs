using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.ViewModels.Notification;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class NotificationController : BaseController
{
    private readonly INotificationService _notificationService;

    public NotificationController(
        INotificationService notificationService,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _notificationService = notificationService;
    }

    public async Task<IActionResult> Index()
    {
        var notifications = await _notificationService.GetUserNotificationsAsync(CurrentUser.UserId, 100);

        var model = new NotificationIndexViewModel
        {
            UnreadCount = notifications.Count(x => !x.IsRead),
            Notifications = notifications.Select(x => new NotificationItemViewModel
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        if (!await _notificationService.CanManageNotificationAsync(id, CurrentUser.UserId))
            return Forbid();

        await _notificationService.MarkAsReadAsync(id, CurrentUser.UserId);
        var unreadCount = await _notificationService.GetUnreadCountAsync(CurrentUser.UserId);

        return Json(new { success = true, unreadCount });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _notificationService.MarkAllAsReadAsync(CurrentUser.UserId);
        return Json(new { success = true, unreadCount = 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _notificationService.CanManageNotificationAsync(id, CurrentUser.UserId))
            return Forbid();

        await _notificationService.DeleteAsync(id, CurrentUser.UserId);
        var unreadCount = await _notificationService.GetUnreadCountAsync(CurrentUser.UserId);

        return Json(new { success = true, unreadCount });
    }
}