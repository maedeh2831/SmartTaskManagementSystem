/*
| Module      : Gamification
| Entity      : UserWallet
| Purpose     : مدیریت کیف پول نقطه‌های پاداش کاربر
*/

namespace SmartTask.Web.Models.Entities
{
    public class UserWallet : BaseEntity
    {
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int TotalPoints { get; set; } = 0;
        public int AvailablePoints { get; set; } = 0;
        public int SpentPoints { get; set; } = 0;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<WalletTransaction> Transactions { get; set; } = new List<WalletTransaction>();
    }
}
