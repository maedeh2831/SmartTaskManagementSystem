/*
| Module      : Gamification
| Service     : PurchaseService
| Purpose     : مدیریت خریداری و پرداخت (با لاک برای جلوگیری از double-spend)
*/

using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.ViewModels.Gamification;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace SmartTask.Web.Services.Gamification
{
    public class PurchaseService : IPurchaseService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PurchaseService> _logger;
        private static readonly ConcurrentDictionary<int, object> _userLocks = new();

        public PurchaseService(ApplicationDbContext context, ILogger<PurchaseService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool Success, string Message, int RemainingPoints)> PurchaseItemAsync(int userId, int itemId, int quantity = 1)
        {
            // Get or create lock for this user
            object userLock = _userLocks.GetOrAdd(userId, new object());

            lock (userLock)
            {
                try
                {
                    // Use transaction for database operations
                    using (var transaction = _context.Database.BeginTransaction())
                    {
                        // Get marketplace item
                        var item = _context.Set<MarketplaceItem>()
                            .FirstOrDefault(x => x.Id == itemId && x.IsActive);

                        if (item == null)
                        {
                            transaction.Rollback();
                            return (false, "Item not found", 0);
                        }

                        // Check availability
                        if (!IsItemAvailable(item))
                        {
                            transaction.Rollback();
                            return (false, "Item is not currently available", 0);
                        }

                        // Check stock
                        if (item.Stock != -1 && item.Stock < quantity)
                        {
                            transaction.Rollback();
                            return (false, $"Insufficient stock. Available: {item.Stock}", 0);
                        }

                        // Get user wallet
                        var wallet = _context.Set<UserWallet>()
                            .FirstOrDefault(x => x.UserId == userId);

                        if (wallet == null)
                        {
                            transaction.Rollback();
                            return (false, "User wallet not found", 0);
                        }

                        int totalCost = item.Price * quantity;

                        // Check balance
                        if (wallet.AvailablePoints < totalCost)
                        {
                            transaction.Rollback();
                            return (false, $"Insufficient points. Required: {totalCost}, Available: {wallet.AvailablePoints}", wallet.AvailablePoints);
                        }

                        // Deduct points from wallet
                        wallet.AvailablePoints -= totalCost;
                        wallet.LastUpdated = DateTime.UtcNow;

                        // Record transaction
                        var transaction_record = new MarketplaceTransaction
                        {
                            UserId = userId,
                            UserWalletId = wallet.Id,
                            MarketplaceItemId = itemId,
                            PointsSpent = totalCost,
                            Quantity = quantity,
                            Status = Models.Entities.TransactionStatus.Completed,
                            TransactionDate = DateTime.UtcNow,
                            CreatedBy = userId.ToString(),
                            CreatedDate = DateTime.UtcNow
                        };

                        _context.Set<MarketplaceTransaction>().Add(transaction_record);

                        // Update or create inventory
                        var inventory = _context.Set<UserInventory>()
                            .FirstOrDefault(x => x.UserId == userId && x.MarketplaceItemId == itemId);

                        if (inventory != null)
                        {
                            inventory.Quantity += quantity;
                            inventory.ChangeDate = DateTime.UtcNow;
                        }
                        else
                        {
                            inventory = new UserInventory
                            {
                                UserId = userId,
                                MarketplaceItemId = itemId,
                                Quantity = quantity,
                                AcquiredDate = DateTime.UtcNow,
                                CreatedBy = userId.ToString(),
                                CreatedDate = DateTime.UtcNow
                            };
                            _context.Set<UserInventory>().Add(inventory);
                        }

                        // Update item stock and sold count
                        if (item.Stock != -1)
                        {
                            item.Stock -= quantity;
                        }
                        item.TotalSold += quantity;
                        item.ChangeDate = DateTime.UtcNow;

                        // Save all changes
                        _context.SaveChanges();
                        transaction.Commit();

                        _logger.LogInformation("User {UserId} purchased item {ItemId} x{Quantity} for {Cost} points",
                            userId, itemId, quantity, totalCost);

                        return (true, "Purchase successful", wallet.AvailablePoints);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during purchase for user {UserId}", userId);
                    return (false, "An error occurred during purchase", 0);
                }
            }
        }

        public async Task<List<UserInventoryDto>> GetUserInventoryAsync(int userId)
        {
            try
            {
                var inventory = await _context.Set<UserInventory>()
                    .Where(x => x.UserId == userId)
                    .Include(x => x.MarketplaceItem)
                    .ToListAsync();

                return inventory.Select(MapToDto).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory for user {UserId}", userId);
                return new List<UserInventoryDto>();
            }
        }

        public async Task<UserInventoryDto> GetInventoryItemAsync(int userId, int itemId)
        {
            try
            {
                var item = await _context.Set<UserInventory>()
                    .Include(x => x.MarketplaceItem)
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.MarketplaceItemId == itemId);

                if (item == null) return null;

                return MapToDto(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory item for user {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> EquipItemAsync(int userId, int inventoryId)
        {
            try
            {
                var inventory = await _context.Set<UserInventory>()
                    .Include(x => x.MarketplaceItem)
                    .FirstOrDefaultAsync(x => x.Id == inventoryId && x.UserId == userId);

                if (inventory == null) return false;

                // در هر دسته فقط یک قلم می‌تواند فعال باشد؛ بقیه غیرفعال می‌شوند
                var category = inventory.MarketplaceItem?.Category;
                if (!string.IsNullOrEmpty(category))
                {
                    var sameCategory = await _context.Set<UserInventory>()
                        .Include(x => x.MarketplaceItem)
                        .Where(x => x.UserId == userId
                                    && x.Id != inventoryId
                                    && x.IsEquipped
                                    && x.MarketplaceItem != null
                                    && x.MarketplaceItem.Category == category)
                        .ToListAsync();

                    foreach (var other in sameCategory)
                    {
                        other.IsEquipped = false;
                        other.EquippedDate = null;
                        other.ChangeDate = DateTime.UtcNow;
                    }
                }

                inventory.IsEquipped = true;
                inventory.EquippedDate = DateTime.UtcNow;
                inventory.ChangeDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} equipped item {ItemId}", userId, inventory.MarketplaceItemId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error equipping item for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> UnequipItemAsync(int userId, int inventoryId)
        {
            try
            {
                var inventory = await _context.Set<UserInventory>()
                    .FirstOrDefaultAsync(x => x.Id == inventoryId && x.UserId == userId);

                if (inventory == null) return false;

                inventory.IsEquipped = false;
                inventory.EquippedDate = null;
                inventory.ChangeDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("User {UserId} unequipped item {ItemId}", userId, inventory.MarketplaceItemId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unequipping item for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> HasItemAsync(int userId, int itemId)
        {
            try
            {
                return await _context.Set<UserInventory>()
                    .AnyAsync(x => x.UserId == userId && x.MarketplaceItemId == itemId && x.Quantity > 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user has item");
                return false;
            }
        }

        private bool IsItemAvailable(MarketplaceItem item)
        {
            if (!item.IsActive) return false;

            if (item.IsLimitedTime)
            {
                var now = DateTime.UtcNow;
                if (item.AvailableFrom != null && now < item.AvailableFrom) return false;
                if (item.AvailableUntil != null && now > item.AvailableUntil) return false;
            }

            return true;
        }

        private UserInventoryDto MapToDto(UserInventory inventory)
        {
            return new UserInventoryDto
            {
                Id = inventory.Id,
                ItemId = inventory.MarketplaceItemId,
                ItemName = inventory.MarketplaceItem?.Name,
                ItemDescription = inventory.MarketplaceItem?.Description,
                ItemIcon = inventory.MarketplaceItem?.Icon,
                ItemColor = inventory.MarketplaceItem?.Color,
                Category = inventory.MarketplaceItem?.Category,
                Rarity = inventory.MarketplaceItem?.Rarity ?? Models.Enums.MarketplaceItemRarity.Common,
                Quantity = inventory.Quantity,
                IsEquipped = inventory.IsEquipped,
                AcquiredDate = inventory.AcquiredDate,
                EquippedDate = inventory.EquippedDate
            };
        }
    }
}
