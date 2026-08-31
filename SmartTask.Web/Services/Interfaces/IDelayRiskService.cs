using SmartTask.Web.Models.ViewModels.Risk;
using SmartTask.Web.Models.ViewModels.Search;

namespace SmartTask.Web.Services.Interfaces;

public interface IDelayRiskService
{
    Task<DelayRiskViewModel?> GetRiskOverviewAsync(int projectId, int currentUserId);

    /// <summary>
    /// تحلیل ترکیبی: الگوریتم + LLM. امتیاز الگوریتمی + تحلیل AI.
    /// </summary>
    Task<DelayRiskViewModel?> GetRiskOverviewWithAiAsync(int projectId, int currentUserId);

    Task<string> GenerateNarrativeAsync(int projectId, int currentUserId);
}