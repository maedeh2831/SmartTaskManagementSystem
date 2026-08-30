/*
| Module      : Gamification
| Service     : EconomyAnalysisService
| Purpose     : تحلیل اقتصاد بازار
*/

using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace SmartTask.Web.Services.Gamification
{
    public class EconomyAnalysisService : IEconomyAnalysisService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EconomyAnalysisService> _logger;

        public EconomyAnalysisService(ApplicationDbContext context, ILogger<EconomyAnalysisService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Dictionary<string, object>> GetMarketplaceMetricsAsync()
        {
            try
            {
                var totalItems = await _context.Set<MarketplaceItem>().CountAsync(x => x.IsActive);
                var totalSales = await _context.Set<MarketplaceTransaction>()
                    .Where(x => x.Status == Models.Entities.TransactionStatus.Completed)
                    .CountAsync();
                var totalRevenue = await _context.Set<MarketplaceTransaction>()
                    .Where(x => x.Status == Models.Entities.TransactionStatus.Completed)
                    .SumAsync(x => (long)x.PointsSpent);

                var uniqueBuyers = await _context.Set<MarketplaceTransaction>()
                    .Where(x => x.Status == Models.Entities.TransactionStatus.Completed)
                    .Select(x => x.UserId)
                    .Distinct()
                    .CountAsync();

                var averageTransactionValue = totalSales > 0 ? (decimal)totalRevenue / totalSales : 0;

                return new Dictionary<string, object>
                {
                    { "TotalItems", totalItems },
                    { "TotalSales", totalSales },
                    { "TotalRevenue", totalRevenue },
                    { "UniqueBuyers", uniqueBuyers },
                    { "AverageTransactionValue", Math.Round(averageTransactionValue, 2) },
                    { "Timestamp", DateTime.UtcNow }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating marketplace metrics");
                return new Dictionary<string, object> { { "Error", ex.Message } };
            }
        }

        public async Task<Dictionary<string, object>> GetUserEconomyStatsAsync(int userId)
        {
            try
            {
                var wallet = await _context.Set<UserWallet>()
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                if (wallet == null)
                    return new Dictionary<string, object> { { "Error", "User wallet not found" } };

                var totalSpent = await _context.Set<MarketplaceTransaction>()
                    .Where(x => x.UserId == userId && x.Status == Models.Entities.TransactionStatus.Completed)
                    .SumAsync(x => (long)x.PointsSpent);

                var purchaseCount = await _context.Set<MarketplaceTransaction>()
                    .Where(x => x.UserId == userId && x.Status == Models.Entities.TransactionStatus.Completed)
                    .CountAsync();

                var inventoryCount = await _context.Set<UserInventory>()
                    .Where(x => x.UserId == userId)
                    .CountAsync();

                var equippedCount = await _context.Set<UserInventory>()
                    .Where(x => x.UserId == userId && x.IsEquipped)
                    .CountAsync();

                return new Dictionary<string, object>
                {
                    { "UserId", userId },
                    { "CurrentPoints", wallet.AvailablePoints },
                    { "TotalPointsEarned", wallet.TotalPoints },
                    { "TotalPointsSpent", totalSpent },
                    { "PurchaseCount", purchaseCount },
                    { "InventoryItemCount", inventoryCount },
                    { "EquippedItemCount", equippedCount },
                    { "Timestamp", DateTime.UtcNow }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating user economy stats for user {UserId}", userId);
                return new Dictionary<string, object> { { "Error", ex.Message } };
            }
        }

        public async Task<List<(string ItemName, int TotalSold, int Revenue)>> GetTopSellingItemsAsync(int take = 10)
        {
            try
            {
                var topItems = await _context.Set<MarketplaceItem>()
                    .AsNoTracking()
                    .Where(x => x.IsActive)
                    .OrderByDescending(x => x.TotalSold)
                    .Take(take)
                    .Select(x => new
                    {
                        x.Name,
                        x.TotalSold,
                        Revenue = x.Price * x.TotalSold
                    })
                    .ToListAsync();

                return topItems
                    .Select(x => (x.Name, x.TotalSold, x.Revenue))
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving top selling items");
                return new List<(string, int, int)>();
            }
        }

        public async Task<Dictionary<string, int>> GetCategoryDistributionAsync()
        {
            try
            {
                var distribution = await _context.Set<MarketplaceTransaction>()
                    .Where(x => x.Status == Models.Entities.TransactionStatus.Completed)
                    .Include(x => x.MarketplaceItem)
                    .GroupBy(x => x.MarketplaceItem.Category)
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .ToListAsync();

                return distribution.ToDictionary(x => x.Category, x => x.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating category distribution");
                return new Dictionary<string, int>();
            }
        }
    }
}
