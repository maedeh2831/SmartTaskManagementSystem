/*
| Module      : Gamification
| Interface   : IEconomyAnalysisService
| Purpose     : تحلیل اقتصاد بازار
*/

namespace SmartTask.Web.Services.Gamification
{
    public interface IEconomyAnalysisService
    {
        Task<Dictionary<string, object>> GetMarketplaceMetricsAsync();
        Task<Dictionary<string, object>> GetUserEconomyStatsAsync(int userId);
        Task<List<(string ItemName, int TotalSold, int Revenue)>> GetTopSellingItemsAsync(int take = 10);
        Task<Dictionary<string, int>> GetCategoryDistributionAsync();
    }
}
