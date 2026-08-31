/*
| Module      : Gamification
| Interface   : IAbuseDetectionEngine
| Purpose     : تعریف قرارداد موتور تشخیص سوء استفاده
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Services.Gamification
{
    public interface IAbuseDetectionEngine
    {
        Task ScanUserActivityAsync(int userId);
        Task<List<dynamic>> GetPendingReportsAsync();
        Task<dynamic> GetReportAsync(int reportId);
        Task ResolveReportAsync(int reportId, AbuseReportStatus status, string notes, int? reviewedByUserId = null);
        Task RefundRewardAsync(int reportId, int amount);
        Task SuspendRewardsAsync(int reportId, DateTime until);
        Task ResumeRewardsAsync(int userId);
        Task<bool> IsUserSuspendedAsync(int userId);
    }
}
