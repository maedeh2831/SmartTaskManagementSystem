/*
| Module      : Gamification
| DTO         : MarketplaceItemDto
| Purpose     : نمایش اطلاعات اقلام بازار
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Gamification
{
    public class MarketplaceItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }

        public string Category { get; set; }
        public MarketplaceItemRarity Rarity { get; set; }

        public int Price { get; set; }
        public int Stock { get; set; }
        public int TotalSold { get; set; }

        public bool IsLimitedTime { get; set; }
        public DateTime? AvailableFrom { get; set; }
        public DateTime? AvailableUntil { get; set; }

        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }

        public bool IsOwned { get; set; } // Set by API based on user inventory
        public int OwnedQuantity { get; set; }
        public bool IsEquipped { get; set; }
    }
}
