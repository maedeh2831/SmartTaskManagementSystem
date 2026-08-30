/*
| Module      : Gamification
| Interface   : IMarketplaceService
| Purpose     : مدیریت اقلام بازار
*/

using SmartTask.Web.Models.ViewModels.Gamification;

namespace SmartTask.Web.Services.Gamification
{
    public interface IMarketplaceService
    {
        // Items
        Task<List<MarketplaceItemDto>> GetAllItemsAsync(string? category = null, int skip = 0, int take = 20);
        Task<MarketplaceItemDto> GetItemByIdAsync(int itemId);
        Task<List<string>> GetCategoriesAsync();

        // Admin operations
        Task<MarketplaceItemDto> CreateItemAsync(MarketplaceItemDto itemDto);
        Task<MarketplaceItemDto> UpdateItemAsync(int itemId, MarketplaceItemDto itemDto);
        Task<bool> DeleteItemAsync(int itemId);
        Task<bool> UpdateStockAsync(int itemId, int newStock);

        // Availability
        Task<bool> IsItemAvailableAsync(int itemId);
        Task<int> GetAvailableStockAsync(int itemId);
    }
}
