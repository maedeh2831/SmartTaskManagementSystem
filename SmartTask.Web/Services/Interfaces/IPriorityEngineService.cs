using SmartTask.Web.Models.ViewModels.Priority;

namespace SmartTask.Web.Services.Interfaces;

public interface IPriorityEngineService
{
    Task<SmartPriorityViewModel> GetSuggestionAsync(int taskId, int currentUserId);
    Task ApplySuggestionAsync(int taskId, int currentUserId);
}