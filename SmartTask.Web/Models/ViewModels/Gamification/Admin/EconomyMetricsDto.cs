/*
| Module      : Gamification
| DTO         : EconomyMetricsDto
| Purpose     : معیارهای اقتصاد گیمیفیکیشن
*/

namespace SmartTask.Web.Models.ViewModels.Gamification.Admin
{
    public class EconomyMetricsDto
    {
        public long TotalXpDistributed { get; set; }
        public long TotalMomentumCirculating { get; set; }
        public decimal AverageMomentumPerUser { get; set; }
        public decimal PurchaseVelocity { get; set; } // Purchases per day
        public int ActiveUsersInLastWeek { get; set; }
        public int ActiveUsersInLastMonth { get; set; }
        public decimal AverageXpPerActiveUser { get; set; }
        public decimal AchievementUnlockRate { get; set; } // Percentage
        public int TotalAchievementsUnlocked { get; set; }
        public int MarketplaceTransactionsLastWeek { get; set; }
        public int MarketplaceTransactionsLastMonth { get; set; }
        public DateTime CalculatedAt { get; set; }
    }
}
