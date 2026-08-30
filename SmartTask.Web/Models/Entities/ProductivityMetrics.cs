/*
| Module      : Gamification
| Entity      : ProductivityMetrics
| Purpose     : تعریف متریکس بهره‌وری کاربر (نمرات و آمارات)
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities
{
    public class ProductivityMetrics : BaseEntity
    {
        public int UserId { get; set; }
        public int WorkspaceId { get; set; }

        // Score Metrics (0-100)
        public double ProductivityScore { get; set; } // Overall score
        public double TaskCompletionRate { get; set; } // Percentage completed
        public double OnTimeDeliveryRate { get; set; } // Percentage on-time
        public double ConsistencyRate { get; set; } // Worked days / total days
        public double QualityScore { get; set; } // Tasks without reopens

        // Activity Metrics
        public int TotalTasksAssigned { get; set; }
        public int TotalTasksCompleted { get; set; }
        public int OnTimeTasksCompleted { get; set; }
        public int OverdueTasksCompleted { get; set; }
        public int TasksReopened { get; set; }
        public int WorkedDaysThisPeriod { get; set; }
        public int TotalDaysInPeriod { get; set; }

        // Streak Information
        public int CurrentStreak { get; set; } // Consecutive days of activity
        public int LongestStreak { get; set; }
        public DateTime LastActivityDate { get; set; }

        // Tier Classification
        public ProductivityTier CurrentTier { get; set; } = ProductivityTier.Bronze;

        // Period Information
        public DateTime PeriodStartDate { get; set; }
        public DateTime PeriodEndDate { get; set; }
        public bool IsCurrentPeriod { get; set; } = true;

        // Relationships
        public virtual ApplicationUser User { get; set; }
        public virtual Workspace Workspace { get; set; }
        public virtual ICollection<ProductivityScoreHistory> ScoreHistory { get; set; } = new List<ProductivityScoreHistory>();
    }
}
