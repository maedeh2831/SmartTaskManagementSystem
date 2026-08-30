/*
| Module      : Gamification
| Entity      : MarketplaceItem
| Purpose     : تعریف اقلام بازار (آیتم‌های قابل خریداری)
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities
{
    public class MarketplaceItem : BaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }

        public string Category { get; set; } // Avatar Border, Badge, Theme, Perk, etc.
        public MarketplaceItemRarity Rarity { get; set; } = MarketplaceItemRarity.Common;

        public int Price { get; set; } // Points cost
        public int Stock { get; set; } // -1 for unlimited
        public int TotalSold { get; set; } = 0;

        public bool IsLimitedTime { get; set; } = false;
        public DateTime? AvailableFrom { get; set; }
        public DateTime? AvailableUntil { get; set; }

        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; } = 0;

        // Navigation
        public ICollection<UserInventory> UserInventories { get; set; } = new List<UserInventory>();
        public ICollection<MarketplaceTransaction> Transactions { get; set; } = new List<MarketplaceTransaction>();
    }
}
