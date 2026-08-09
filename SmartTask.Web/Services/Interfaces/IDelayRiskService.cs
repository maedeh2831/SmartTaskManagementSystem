using SmartTask.Web.Models.ViewModels.Risk;

namespace SmartTask.Web.Services.Interfaces;

public interface IDelayRiskService
{
    Task<DelayRiskViewModel?> GetRiskOverviewAsync(int projectId, int currentUserId);
    Task<string> GenerateNarrativeAsync(int projectId, int currentUserId);
}