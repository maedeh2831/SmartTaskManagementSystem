/*
| Module      : Gamification
| DTO         : UserInventoryDto
| Purpose     : نمایش موجودی کاربر
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Gamification
{
    public class UserInventoryDto
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public string ItemIcon { get; set; }
        public string ItemColor { get; set; }
        public string Category { get; set; }
        public MarketplaceItemRarity Rarity { get; set; }

        public int Quantity { get; set; }
        public bool IsEquipped { get; set; }
        public DateTime AcquiredDate { get; set; }
        public DateTime? EquippedDate { get; set; }
    }
}
