using Microsoft.AspNetCore.Mvc.Rendering;
using SmartTask.Web.Models.ViewModels.TaskTrade;

namespace SmartTask.Web.Services.Interfaces;

public interface ITaskTradeService
{
    Task<TradeModalDataViewModel> GetModalDataAsync(int taskId, int currentUserId);
    Task<List<SelectListItem>> GetUserTasksAsync(int projectId, int userId, int excludeTaskId);
    Task<(bool success, string? error)> CreateRequestAsync(
        int projectId, int requesterUserId, int requesterTaskId,
        int targetUserId, int? targetTaskId, string? message);
    Task<TaskTradeIndexViewModel> GetProjectRequestsAsync(int projectId, int currentUserId);
    Task<(bool success, string? error)> AcceptAsync(int requestId, int currentUserId);
    Task<(bool success, string? error)> RejectAsync(int requestId, int currentUserId);
    Task<(bool success, string? error)> CancelAsync(int requestId, int currentUserId);
}