using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SmartTask.Web.Hubs;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Chat;
using SmartTask.Web.Services.Files;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers;

[Authorize]
public class ChatController : BaseController
{
    private const long MaxAttachmentBytes = 10 * 1024 * 1024;

    private static readonly string[] AllowedExtensions =
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp",
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".csv", ".zip", ".rar", ".7z"
    };

    private static readonly string[] ImageExtensions =
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp"
    };

    private readonly IChatService _chatService;
    private readonly IFileUploadService _fileUploadService;
    private readonly IDateFormatService _dateFormatService;
    private readonly IHubContext<ChatHub> _hubContext;
    private readonly IWebpushrService _webpushrService;

    public ChatController(
        IChatService chatService,
        IFileUploadService fileUploadService,
        IDateFormatService dateFormatService,
        IHubContext<ChatHub> hubContext,
        IWebpushrService webpushrService,
        ICurrentUserService currentUser)
        : base(currentUser)
    {
        _chatService = chatService;
        _fileUploadService = fileUploadService;
        _dateFormatService = dateFormatService;
        _hubContext = hubContext;
        _webpushrService = webpushrService;
    }

    public async Task<IActionResult> Index(int? projectId)
    {
        var userId = CurrentUser.UserId;

        var chats = await _chatService.GetChatListAsync(userId);

        var model = new ChatIndexViewModel
        {
            CurrentUserId = userId,
            CurrentUserName = CurrentUser.FullName,
            IsJalali = _dateFormatService.IsJalali,
            Chats = chats
        };

        var activeId = projectId ?? chats.FirstOrDefault()?.ProjectId;

        if (activeId.HasValue)
        {
            model.ActiveRoom = await _chatService.GetRoomAsync(activeId.Value, userId);

            if (model.ActiveRoom == null && projectId.HasValue)
            {
                TempData["Error"] = "شما عضو این پروژه نیستید.";
                return RedirectToAction(nameof(Index));
            }

            if (model.ActiveRoom != null)
            {
                await _chatService.MarkAsReadAsync(model.ActiveRoom.ProjectId, userId);

                var active = model.Chats.FirstOrDefault(x => x.ProjectId == model.ActiveRoom.ProjectId);
                if (active != null)
                    active.UnreadCount = 0;
            }
        }

        return View(model);
    }

    /// <summary>بارگذاری اتاق گفتگو به‌صورت JSON هنگام جابه‌جایی بین گروه‌ها.</summary>
    [HttpGet]
    public async Task<IActionResult> Room(int projectId)
    {
        var room = await _chatService.GetRoomAsync(projectId, CurrentUser.UserId);

        if (room == null)
            return Forbid();

        await _chatService.MarkAsReadAsync(projectId, CurrentUser.UserId);

        return Json(room);
    }

    /// <summary>پیمایش تاریخچه؛ پیام‌های قدیمی‌تر از beforeId.</summary>
    [HttpGet]
    public async Task<IActionResult> History(int projectId, int beforeId, int take = 40)
    {
        if (!await _chatService.IsMemberAsync(projectId, CurrentUser.UserId))
            return Forbid();

        take = Math.Clamp(take, 1, 100);

        var messages = await _chatService.GetMessagesAsync(projectId, beforeId, take);

        return Json(new { messages, hasMore = messages.Count == take });
    }

    [HttpGet]
    public async Task<IActionResult> Search(int projectId, string term)
    {
        if (!await _chatService.IsMemberAsync(projectId, CurrentUser.UserId))
            return Forbid();

        var messages = await _chatService.SearchAsync(projectId, term);

        return Json(messages);
    }

    [HttpGet]
    public async Task<IActionResult> Members(int projectId)
    {
        if (!await _chatService.IsMemberAsync(projectId, CurrentUser.UserId))
            return Forbid();

        var members = await _chatService.GetMembersAsync(projectId);

        return Json(members);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(MaxAttachmentBytes + 1024 * 1024)]
    public async Task<IActionResult> Upload(int projectId, IFormFile file, string? caption, int? replyToMessageId)
    {
        var userId = CurrentUser.UserId;

        if (!await _chatService.IsMemberAsync(projectId, userId))
            return Forbid();

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "فایلی انتخاب نشده است." });

        if (file.Length > MaxAttachmentBytes)
            return BadRequest(new { message = "حجم فایل نباید بیشتر از ۱۰ مگابایت باشد." });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
            return BadRequest(new { message = "فرمت این فایل مجاز نیست." });

        var path = await _fileUploadService.SaveFileAsync(file, $"chat/{projectId}");

        var type = ImageExtensions.Contains(extension)
            ? ChatMessageType.Image
            : ChatMessageType.File;

        var message = await _chatService.SendMessageAsync(
            projectId,
            userId,
            caption ?? string.Empty,
            replyToMessageId,
            type,
            path,
            Path.GetFileName(file.FileName),
            file.Length);

        await _hubContext.Clients
            .Group(ChatHub.GetProjectGroupName(projectId))
            .SendAsync("ReceiveMessage", message);

        // Push Notification برای سایر اعضا (متن خالی یعنی فقط فایل ارسال شده)
        var pushBody = string.IsNullOrWhiteSpace(message.Content)
            ? (message.Type == ChatMessageType.Image
                ? "یک تصویر ارسال کرد"
                : $"یک فایل ارسال کرد: {message.AttachmentName}")
            : message.Content;

        await _webpushrService.SendChatMessagePushAsync(
            projectId,
            userId,
            message.SenderName,
            pushBody);

        return Json(message);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAsRead(int projectId)
    {
        await _chatService.MarkAsReadAsync(projectId, CurrentUser.UserId);

        return Json(new { success = true });
    }
}
