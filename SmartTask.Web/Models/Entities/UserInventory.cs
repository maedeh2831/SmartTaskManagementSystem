/*
| Module      : Gamification
| Entity      : UserInventory
| Purpose     : مدیریت موجودی کاربر (اقلامی که کاربر خریده است)
*/

namespace SmartTask.Web.Models.Entities
{
    public class UserInventory : BaseEntity
    {
        public int UserId { get; set; }
        public int MarketplaceItemId { get; set; }

        public int Quantity { get; set; } = 1;
        public bool IsEquipped { get; set; } = false;

        public DateTime AcquiredDate { get; set; } = DateTime.UtcNow;
        public DateTime? EquippedDate { get; set; }

        // Navigation
        public ApplicationUser User { get; set; }
        public MarketplaceItem MarketplaceItem { get; set; }
    }
}
