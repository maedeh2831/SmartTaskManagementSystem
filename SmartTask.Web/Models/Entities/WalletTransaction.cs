/*
| Module      : Gamification
| Entity      : WalletTransaction
| Purpose     : ثبت تراکنش‌های کیف پول
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities
{
    public class WalletTransaction : BaseEntity
    {
        public int UserWalletId { get; set; }
        public UserWallet UserWallet { get; set; }

        public int UserProgressionId { get; set; }
        public UserProgression UserProgression { get; set; }

        public int Amount { get; set; }
        public TransactionType TransactionType { get; set; }
        public string Description { get; set; }
        public int? RelatedTaskId { get; set; }
        public int? RelatedAchievementId { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    }
}
