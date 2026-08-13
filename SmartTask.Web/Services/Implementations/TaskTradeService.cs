using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.TaskTrade;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Services.Implementations;

public class TaskTradeService : ITaskTradeService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public TaskTradeService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<TradeModalDataViewModel> GetModalDataAsync(int taskId, int currentUserId)
    {
        var task = await _context.TaskItems
            .Include(t => t.UserStory)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
            throw new InvalidOperationException("Task یافت نشد.");

        var projectId = task.UserStory.ProjectId;

        var members = await _context.ProjectMembers
            .Where(x => x.ProjectId == projectId && x.ViewState && x.ApplicationUserId != currentUserId)
            .Include(x => x.ApplicationUser)
            .ToListAsync();

        return new TradeModalDataViewModel
        {
            TaskId = taskId,
            ProjectId = projectId,
            ProjectMembers = members
                .Select(x => new SelectListItem { Value = x.ApplicationUserId.ToString(), Text = x.ApplicationUser.FullName })
                .ToList()
        };
    }

    public async Task<List<SelectListItem>> GetUserTasksAsync(int projectId, int userId, int excludeTaskId)
    {
        var tasks = await _context.TaskItems
            .Where(t => t.UserStory.ProjectId == projectId
                && t.ViewState
                && t.Id != excludeTaskId
                && t.Status != TaskStatusType.Done
                && t.Status != TaskStatusType.Cancelled
                && t.Assignments.Any(a => a.ApplicationUserId == userId))
            .OrderBy(t => t.Title)
            .Select(t => new { t.Id, t.Title })
            .ToListAsync();

        return tasks
            .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Title })
            .ToList();
    }

    public async Task<(bool success, string? error)> CreateRequestAsync(
        int projectId, int requesterUserId, int requesterTaskId,
        int targetUserId, int? targetTaskId, string? message)
    {
        if (requesterUserId == targetUserId)
            return (false, "نمی‌توانید با خودتان درخواست ترید ثبت کنید.");

        var requesterOwnsTask = await _context.TaskAssignments
            .AnyAsync(a => a.TaskItemId == requesterTaskId && a.ApplicationUserId == requesterUserId && a.ViewState);

        if (!requesterOwnsTask)
            return (false, "این Task به شما تخصیص داده نشده است.");

        if (targetTaskId.HasValue)
        {
            var targetOwnsTask = await _context.TaskAssignments
                .AnyAsync(a => a.TaskItemId == targetTaskId.Value && a.ApplicationUserId == targetUserId && a.ViewState);

            if (!targetOwnsTask)
                return (false, "Task انتخاب‌شده به این عضو تخصیص ندارد.");
        }

        var duplicatePending = await _context.TaskTradeRequests
            .AnyAsync(x => x.RequesterTaskId == requesterTaskId
                && x.Status == TradeRequestStatusType.Pending
                && x.ViewState);

        if (duplicatePending)
            return (false, "برای این Task قبلاً یک درخواست ترید در انتظار وجود دارد.");

        var request = new TaskTradeRequest
        {
            ProjectId = projectId,
            RequesterUserId = requesterUserId,
            TargetUserId = targetUserId,
            RequesterTaskId = requesterTaskId,
            TargetTaskId = targetTaskId,
            Message = message,
            Status = TradeRequestStatusType.Pending,
            CreatedDate = DateTime.Now,
            ViewState = true
        };

        _context.TaskTradeRequests.Add(request);
        await _context.SaveChangesAsync();

        var requesterTaskTitle = (await _context.TaskItems.FindAsync(requesterTaskId))?.Title ?? "";

        await _notificationService.CreateAsync(
            targetUserId,
            "درخواست ترید تسک",
            targetTaskId.HasValue
                ? $"یک درخواست مبادله تسک برای «{requesterTaskTitle}» دریافت کرده‌اید."
                : $"یک درخواست واگذاری تسک «{requesterTaskTitle}» به شما ارسال شده است.",
            NotificationType.System);

        return (true, null);
    }

    public async Task<TaskTradeIndexViewModel> GetProjectRequestsAsync(int projectId, int currentUserId)
    {
        var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == projectId);

        var requests = await _context.TaskTradeRequests
            .Where(x => x.ProjectId == projectId && x.ViewState
                && (x.RequesterUserId == currentUserId || x.TargetUserId == currentUserId))
            .Include(x => x.RequesterUser)
            .Include(x => x.TargetUser)
            .Include(x => x.RequesterTask)
            .Include(x => x.TargetTask)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync();

        var vm = new TaskTradeIndexViewModel
        {
            ProjectId = projectId,
            ProjectName = project?.Name ?? "",
        };

        foreach (var r in requests)
        {
            var item = new TaskTradeItemViewModel
            {
                Id = r.Id,
                RequesterName = r.RequesterUser.FullName,
                TargetName = r.TargetUser.FullName,
                RequesterTaskTitle = r.RequesterTask.Title,
                TargetTaskTitle = r.TargetTask?.Title,
                Message = r.Message,
                Status = r.Status,
                CreateDate = r.CreatedDate,
                IsIncoming = r.TargetUserId == currentUserId
            };

            if (item.IsIncoming)
                vm.Incoming.Add(item);
            else
                vm.Outgoing.Add(item);
        }

        return vm;
    }

    public async Task<(bool success, string? error)> AcceptAsync(int requestId, int currentUserId)
    {
        var request = await _context.TaskTradeRequests.FirstOrDefaultAsync(x => x.Id == requestId && x.ViewState);

        if (request == null)
            return (false, "درخواست یافت نشد.");

        if (request.TargetUserId != currentUserId)
            return (false, "شما اجازه پاسخ به این درخواست را ندارید.");

        if (request.Status != TradeRequestStatusType.Pending)
            return (false, "این درخواست قبلاً پاسخ داده شده است.");

        // ===== جابه‌جایی واقعی Assignment ها =====
        await SwapAssignmentAsync(request.RequesterTaskId, request.RequesterUserId, request.TargetUserId);

        if (request.TargetTaskId.HasValue)
            await SwapAssignmentAsync(request.TargetTaskId.Value, request.TargetUserId, request.RequesterUserId);

        request.Status = TradeRequestStatusType.Accepted;
        request.ResponseDate = DateTime.Now;
        request.ChangeDate = DateTime.Now;

        await _context.SaveChangesAsync();

        await _notificationService.CreateAsync(
            request.RequesterUserId,
            "ترید تسک تأیید شد",
            "درخواست ترید شما تأیید و اعمال شد.",
            NotificationType.System);

        return (true, null);
    }

    private async Task SwapAssignmentAsync(int taskId, int fromUserId, int toUserId)
    {
        var assignment = await _context.TaskAssignments
            .FirstOrDefaultAsync(a => a.TaskItemId == taskId && a.ApplicationUserId == fromUserId && a.ViewState);

        if (assignment != null)
        {
            assignment.ViewState = false;
            assignment.ChangeDate = DateTime.Now;
        }

        var alreadyAssigned = await _context.TaskAssignments
            .AnyAsync(a => a.TaskItemId == taskId && a.ApplicationUserId == toUserId && a.ViewState);

        if (!alreadyAssigned)
        {
            _context.TaskAssignments.Add(new TaskAssignment
            {
                TaskItemId = taskId,
                ApplicationUserId = toUserId,
                AssignedDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                ViewState = true
            });
        }
    }

    public async Task<(bool success, string? error)> RejectAsync(int requestId, int currentUserId)
    {
        var request = await _context.TaskTradeRequests.FirstOrDefaultAsync(x => x.Id == requestId && x.ViewState);

        if (request == null)
            return (false, "درخواست یافت نشد.");

        if (request.TargetUserId != currentUserId)
            return (false, "شما اجازه پاسخ به این درخواست را ندارید.");

        if (request.Status != TradeRequestStatusType.Pending)
            return (false, "این درخواست قبلاً پاسخ داده شده است.");

        request.Status = TradeRequestStatusType.Rejected;
        request.ResponseDate = DateTime.Now;
        request.ChangeDate = DateTime.Now;

        await _context.SaveChangesAsync();

        await _notificationService.CreateAsync(
            request.RequesterUserId,
            "ترید تسک رد شد",
            "درخواست ترید شما رد شد.",
            NotificationType.System);

        return (true, null);
    }

    public async Task<(bool success, string? error)> CancelAsync(int requestId, int currentUserId)
    {
        var request = await _context.TaskTradeRequests.FirstOrDefaultAsync(x => x.Id == requestId && x.ViewState);

        if (request == null)
            return (false, "درخواست یافت نشد.");

        if (request.RequesterUserId != currentUserId)
            return (false, "شما اجازه لغو این درخواست را ندارید.");

        if (request.Status != TradeRequestStatusType.Pending)
            return (false, "این درخواست قابل لغو نیست.");

        request.Status = TradeRequestStatusType.Cancelled;
        request.ResponseDate = DateTime.Now;
        request.ChangeDate = DateTime.Now;

        await _context.SaveChangesAsync();

        return (true, null);
    }
}