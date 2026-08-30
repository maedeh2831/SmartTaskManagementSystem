/*
| Module      : Gamification
| Entity      : MarketplaceTransaction
| Purpose     : ثبت تراکنش‌های خریداری از بازار
*/

namespace SmartTask.Web.Models.Entities
{
    public class MarketplaceTransaction : BaseEntity
    {
        public int UserId { get; set; }
        public int UserWalletId { get; set; }
        public int MarketplaceItemId { get; set; }

        public int PointsSpent { get; set; }
        public int Quantity { get; set; } = 1;

        public TransactionStatus Status { get; set; } = TransactionStatus.Completed;
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public ApplicationUser User { get; set; }
        public UserWallet UserWallet { get; set; }
        public MarketplaceItem MarketplaceItem { get; set; }
    }

    public enum TransactionStatus
    {
        Pending = 1,
        Completed = 2,
        Failed = 3,
        Refunded = 4
    }
}
