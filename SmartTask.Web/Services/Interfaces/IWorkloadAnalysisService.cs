using SmartTask.Web.Models.ViewModels.Workload;

namespace SmartTask.Web.Services.Interfaces;

public interface IWorkloadAnalysisService
{
    Task<WorkloadIndexViewModel?> GetWorkloadAsync(int projectId, int currentUserId);
    Task UpdateCapacityAsync(int projectMemberId, int weeklyCapacityHours);
    Task<int> GetUserUtilizationAsync(int projectId, int userId);   // ← جدید
}