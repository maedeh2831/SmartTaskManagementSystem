/*
| Module      : Gamification
| Interface   : IGamificationAnalyticsService
| Purpose     : تعریف قرارداد خدمات تحلیلی گیمیفیکیشن
*/

using SmartTask.Web.Models.ViewModels.Gamification.Admin;

namespace SmartTask.Web.Services.Gamification
{
    public interface IGamificationAnalyticsService
    {
        Task<EconomyMetricsDto> GetEconomyMetricsAsync();
        Task<List<dynamic>> GetDailyActiveUsersAsync(int days = 30);
        Task<List<dynamic>> GetAverageXpPerUserAsync(int days = 30);
        Task<List<dynamic>> GetAchievementUnlockRatesAsync();
        Task<List<dynamic>> GetLevelDistributionAsync();
        Task<UserProgressionAdminDto> GetUserProgressionAdminAsync(int userId);
        Task<List<UserProgressionAdminDto>> GetTopUsersAsync(int limit = 20);
        Task<List<dynamic>> GetMarketplaceMetricsAsync();
    }
}
