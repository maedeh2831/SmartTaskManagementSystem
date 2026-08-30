/*
| Module      : ViewModels/Gamification
| DTO         : ProductivityMetricsDto
| Purpose     : تبدیل متریکس بهره‌وری برای انتقال به کلاینت
*/

namespace SmartTask.Web.Models.ViewModels.Gamification
{
    public class ProductivityMetricsDto
    {
        public int UserId { get; set; }

        // Calculated Scores (0-100)
        public double ProductivityScore { get; set; }
        public double TaskCompletionRate { get; set; }
        public double OnTimeDeliveryRate { get; set; }
        public double ConsistencyRate { get; set; }
        public double QualityScore { get; set; }

        // Streaks
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }

        // Tier
        public int CurrentTier { get; set; } // ProductivityTier enum

        // Activity Stats
        public int TotalTasksAssigned { get; set; }
        public int TotalTasksCompleted { get; set; }
        public int OnTimeTasksCompleted { get; set; }

        public string TierName => GetTierName();
        public string TierBadge => GetTierBadge();
        public string TierColor => GetTierColor();

        private string GetTierName() => CurrentTier switch
        {
            0 => "Bronze",
            1 => "Silver",
            2 => "Gold",
            3 => "Platinum",
            4 => "Diamond",
            _ => "Unranked"
        };

        private string GetTierBadge() => CurrentTier switch
        {
            0 => "🥉",
            1 => "🥈",
            2 => "🥇",
            3 => "💎",
            4 => "👑",
            _ => "❓"
        };

        private string GetTierColor() => CurrentTier switch
        {
            0 => "#CD7F32", // Bronze
            1 => "#C0C0C0", // Silver
            2 => "#FFD700", // Gold
            3 => "#E5E4E2", // Platinum
            4 => "#B9F2FF", // Diamond
            _ => "#808080"  // Gray
        };
    }

    public class ProductivityScoreDto
    {
        public DateTime SnapshotDate { get; set; }
        public double ProductivityScore { get; set; }
        public double TaskCompletionRate { get; set; }
        public double OnTimeDeliveryRate { get; set; }
        public double ConsistencyRate { get; set; }
        public double QualityScore { get; set; }
        public int CurrentStreak { get; set; }
        public int TierAtSnapshot { get; set; }
    }

    public class TeamProductivityDto
    {
        public int TeamId { get; set; }
        public int MemberCount { get; set; }
        public double AverageProductivityScore { get; set; }
        public double AverageTaskCompletionRate { get; set; }
        public double AverageOnTimeDeliveryRate { get; set; }
        public int TotalTasksCompleted { get; set; }
        public double AverageCurrentStreak { get; set; }
    }

    public class BenchmarkMetricsDto
    {
        public double UserScore { get; set; }
        public double WorkspaceAverageScore { get; set; }
        public double UserRankPercentile { get; set; }
        public double ComparisonToAverage { get; set; }
    }

    public class ProductivityDashboardDto
    {
        public ProductivityMetricsDto UserMetrics { get; set; }
        public TeamProductivityDto TeamMetrics { get; set; }
        public BenchmarkMetricsDto BenchmarkMetrics { get; set; }
        public List<ProductivityScoreDto> RecentHistory { get; set; } = new();
    }
}
