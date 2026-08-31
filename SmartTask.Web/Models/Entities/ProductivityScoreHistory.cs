/*
| Module      : Gamification
| Entity      : ProductivityScoreHistory
| Purpose     : ذخیره‌ی تاریخچهٔ نمرات بهره‌وری (اسنپ‌شات روزانه/هفتگی)
*/

namespace SmartTask.Web.Models.Entities
{
    public class ProductivityScoreHistory : BaseEntity
    {
        public int ProductivityMetricsId { get; set; }
        public int UserId { get; set; }

        // Score Snapshot
        public double ProductivityScore { get; set; }
        public double TaskCompletionRate { get; set; }
        public double OnTimeDeliveryRate { get; set; }
        public double ConsistencyRate { get; set; }
        public double QualityScore { get; set; }

        // Activity Snapshot
        public int TasksCompletedThisPeriod { get; set; }
        public int OnTimeTasksThisPeriod { get; set; }
        public int CurrentStreak { get; set; }

        // Date of Snapshot
        public DateTime SnapshotDate { get; set; } = DateTime.UtcNow;
        public string PeriodType { get; set; } = "Daily"; // Daily, Weekly, Monthly

        // Tier at this snapshot
        public int TierAtSnapshot { get; set; }

        // Relationships
        public virtual ProductivityMetrics ProductivityMetrics { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
