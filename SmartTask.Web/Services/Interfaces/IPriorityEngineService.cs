using SmartTask.Web.Models.ViewModels.Priority;

namespace SmartTask.Web.Services.Interfaces;

public interface IPriorityEngineService
{
    Task<SmartPriorityViewModel> GetSuggestionAsync(int taskId, int currentUserId);

    /// <summary>
    /// تحلیل ترکیبی: الگوریتم + LLM. دلایل تکمیلی هوش مصنوعی.
    /// </summary>
    Task<SmartPriorityViewModel> GetSuggestionWithAiAsync(int taskId, int currentUserId);

    Task ApplySuggestionAsync(int taskId, int currentUserId);
}