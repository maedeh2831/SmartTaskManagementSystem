using SmartTask.Web.Models.ViewModels.Health;

namespace SmartTask.Web.Services.Interfaces;

public interface IProjectHealthService
{
    Task<ProjectHealthViewModel?> GetHealthAsync(int projectId, int currentUserId);

    /// <summary>
    /// تحلیل ترکیبی: الگوریتم + LLM. شاخص‌های سلامت + تحلیل هوش مصنوعی.
    /// </summary>
    Task<ProjectHealthViewModel?> GetHealthWithAiAsync(int projectId, int currentUserId);
}