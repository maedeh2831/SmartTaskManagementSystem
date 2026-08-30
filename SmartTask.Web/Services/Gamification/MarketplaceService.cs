/*
| Module      : Gamification
| Service     : MarketplaceService
| Purpose     : مدیریت اقلام بازار
*/

using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.ViewModels.Gamification;
using Microsoft.EntityFrameworkCore;

namespace SmartTask.Web.Services.Gamification
{
    public class MarketplaceService : IMarketplaceService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MarketplaceService> _logger;

        public MarketplaceService(ApplicationDbContext context, ILogger<MarketplaceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<MarketplaceItemDto>> GetAllItemsAsync(string? category = null, int skip = 0, int take = 20)
        {
            try
            {
                var query = _context.Set<MarketplaceItem>()
                    .Where(x => x.IsActive);

                // Filter by category
                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(x => x.Category == category);
                }

                // Filter by availability (limited time items)
                query = query.Where(x => !x.IsLimitedTime ||
                    (x.AvailableFrom <= DateTime.UtcNow && x.AvailableUntil >= DateTime.UtcNow) ||
                    (x.AvailableFrom == null && x.AvailableUntil == null));

                var items = await query
                    .OrderBy(x => x.DisplayOrder)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();

                return items.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving marketplace items");
                return new List<MarketplaceItemDto>();
            }
        }

        public async Task<MarketplaceItemDto> GetItemByIdAsync(int itemId)
        {
            try
            {
                var item = await _context.Set<MarketplaceItem>()
                    .FirstOrDefaultAsync(x => x.Id == itemId && x.IsActive);

                if (item == null)
                {
                    _logger.LogWarning("Marketplace item {ItemId} not found", itemId);
                    return null;
                }

                return MapToDto(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving marketplace item {ItemId}", itemId);
                return null;
            }
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            try
            {
                return await _context.Set<MarketplaceItem>()
                    .Where(x => x.IsActive)
                    .Select(x => x.Category)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving marketplace categories");
                return new List<string>();
            }
        }

        public async Task<MarketplaceItemDto> CreateItemAsync(MarketplaceItemDto itemDto)
        {
            try
            {
                var item = new MarketplaceItem
                {
                    Name = itemDto.Name,
                    Description = itemDto.Description,
                    Icon = itemDto.Icon,
                    Color = itemDto.Color,
                    Category = itemDto.Category,
                    Rarity = itemDto.Rarity,
                    Price = itemDto.Price,
                    Stock = itemDto.Stock,
                    IsLimitedTime = itemDto.IsLimitedTime,
                    AvailableFrom = itemDto.AvailableFrom,
                    AvailableUntil = itemDto.AvailableUntil,
                    IsActive = itemDto.IsActive,
                    DisplayOrder = itemDto.DisplayOrder,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Set<MarketplaceItem>().Add(item);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Created marketplace item {ItemName} with ID {ItemId}", item.Name, item.Id);
                return MapToDto(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating marketplace item");
                throw;
            }
        }

        public async Task<MarketplaceItemDto> UpdateItemAsync(int itemId, MarketplaceItemDto itemDto)
        {
            try
            {
                var item = await _context.Set<MarketplaceItem>().FindAsync(itemId);
                if (item == null)
                {
                    _logger.LogWarning("Marketplace item {ItemId} not found for update", itemId);
                    return null;
                }

                item.Name = itemDto.Name;
                item.Description = itemDto.Description;
                item.Icon = itemDto.Icon;
                item.Color = itemDto.Color;
                item.Category = itemDto.Category;
                item.Rarity = itemDto.Rarity;
                item.Price = itemDto.Price;
                item.Stock = itemDto.Stock;
                item.IsLimitedTime = itemDto.IsLimitedTime;
                item.AvailableFrom = itemDto.AvailableFrom;
                item.AvailableUntil = itemDto.AvailableUntil;
                item.IsActive = itemDto.IsActive;
                item.DisplayOrder = itemDto.DisplayOrder;
                item.ChangeDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated marketplace item {ItemId}", itemId);
                return MapToDto(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating marketplace item {ItemId}", itemId);
                throw;
            }
        }

        public async Task<bool> DeleteItemAsync(int itemId)
        {
            try
            {
                var item = await _context.Set<MarketplaceItem>().FindAsync(itemId);
                if (item == null)
                {
                    _logger.LogWarning("Marketplace item {ItemId} not found for deletion", itemId);
                    return false;
                }

                item.IsActive = false;
                item.ChangeDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Deleted marketplace item {ItemId}", itemId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting marketplace item {ItemId}", itemId);
                return false;
            }
        }

        public async Task<bool> UpdateStockAsync(int itemId, int newStock)
        {
            try
            {
                var item = await _context.Set<MarketplaceItem>().FindAsync(itemId);
                if (item == null)
                {
                    _logger.LogWarning("Marketplace item {ItemId} not found for stock update", itemId);
                    return false;
                }

                item.Stock = newStock;
                item.ChangeDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated stock for item {ItemId} to {Stock}", itemId, newStock);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock for item {ItemId}", itemId);
                return false;
            }
        }

        public async Task<bool> IsItemAvailableAsync(int itemId)
        {
            try
            {
                var item = await _context.Set<MarketplaceItem>()
                    .FirstOrDefaultAsync(x => x.Id == itemId && x.IsActive);

                if (item == null) return false;

                // Check if item is within availability window
                if (item.IsLimitedTime)
                {
                    var now = DateTime.UtcNow;
                    if (item.AvailableFrom != null && now < item.AvailableFrom) return false;
                    if (item.AvailableUntil != null && now > item.AvailableUntil) return false;
                }

                // Check stock
                return item.Stock == -1 || item.Stock > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking item availability for {ItemId}", itemId);
                return false;
            }
        }

        public async Task<int> GetAvailableStockAsync(int itemId)
        {
            try
            {
                var item = await _context.Set<MarketplaceItem>()
                    .FirstOrDefaultAsync(x => x.Id == itemId);

                if (item == null) return 0;

                return item.Stock == -1 ? int.MaxValue : item.Stock;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available stock for item {ItemId}", itemId);
                return 0;
            }
        }

        private MarketplaceItemDto MapToDto(MarketplaceItem item)
        {
            return new MarketplaceItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Icon = item.Icon,
                Color = item.Color,
                Category = item.Category,
                Rarity = item.Rarity,
                Price = item.Price,
                Stock = item.Stock,
                TotalSold = item.TotalSold,
                IsLimitedTime = item.IsLimitedTime,
                AvailableFrom = item.AvailableFrom,
                AvailableUntil = item.AvailableUntil,
                IsActive = item.IsActive,
                DisplayOrder = item.DisplayOrder
            };
        }
    }
}
