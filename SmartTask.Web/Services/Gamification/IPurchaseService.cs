/*
| Module      : Gamification
| Interface   : IPurchaseService
| Purpose     : مدیریت خریداری و پرداخت
*/

using SmartTask.Web.Models.ViewModels.Gamification;

namespace SmartTask.Web.Services.Gamification
{
    public interface IPurchaseService
    {
        Task<(bool Success, string Message, int RemainingPoints)> PurchaseItemAsync(int userId, int itemId, int quantity = 1);
        Task<List<UserInventoryDto>> GetUserInventoryAsync(int userId);
        Task<UserInventoryDto> GetInventoryItemAsync(int userId, int itemId);
        Task<bool> EquipItemAsync(int userId, int inventoryId);
        Task<bool> UnequipItemAsync(int userId, int inventoryId);
        Task<bool> HasItemAsync(int userId, int itemId);
    }
}
