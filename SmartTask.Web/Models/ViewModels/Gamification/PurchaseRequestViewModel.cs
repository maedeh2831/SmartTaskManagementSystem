/*
| Module      : Gamification
| DTO         : PurchaseRequestViewModel
| Purpose     : درخواست خریداری اقلام
*/

namespace SmartTask.Web.Models.ViewModels.Gamification
{
    public class PurchaseRequestViewModel
    {
        public int MarketplaceItemId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class PurchaseResponseViewModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int RemainingPoints { get; set; }
        public UserInventoryDto InventoryItem { get; set; }
    }
}
