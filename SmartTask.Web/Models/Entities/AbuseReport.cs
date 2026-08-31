/*
| Module      : Gamification
| Entity      : AbuseReport
| Purpose     : ثبت و ردیابی گزارش‌های سوء استفاده
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities
{
    public class AbuseReport : BaseEntity
    {
        // Foreign Keys
        public int UserId { get; set; }

        // Report Information
        public AbuseReportType ReportType { get; set; }
        public AbuseReportStatus Status { get; set; } = AbuseReportStatus.Pending;
        public string Description { get; set; }
        public string Evidence { get; set; } // JSON serialized evidence

        // Severity
        public int SeverityScore { get; set; } // 0-100
        public decimal ConfidenceLevel { get; set; } // 0.0-1.0

        // Related Data
        public int? RelatedTaskId { get; set; }
        public int? RelatedProjectId { get; set; }
        public DateTime? IncidentDate { get; set; }

        // Review
        public int? ReviewedByUserId { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public string ReviewNotes { get; set; }

        // Action Taken
        public bool RewardsRefunded { get; set; } = false;
        public bool RewardsSuspended { get; set; } = false;
        public int RefundedAmount { get; set; } = 0;
        public DateTime? SuspensionUntil { get; set; }

        // Metadata
        public string AutoDetectionRule { get; set; } // Which rule triggered this
        public DateTime DetectionDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public ApplicationUser User { get; set; } = null!;
        public ApplicationUser? ReviewedByUser { get; set; }
    }
}
