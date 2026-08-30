/*
| Module      : Gamification
| DTO         : AbuseReportDto
| Purpose     : انتقال اطلاعات گزارش سوء استفاده به سطح ارائه
*/

using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Gamification.Admin
{
    public class AbuseReportDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public AbuseReportType ReportType { get; set; }
        public AbuseReportStatus Status { get; set; }
        public string Description { get; set; }
        public string Evidence { get; set; }
        public int SeverityScore { get; set; }
        public decimal ConfidenceLevel { get; set; }
        public DateTime DetectionDate { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public string ReviewNotes { get; set; }
        public string ReviewedByUserName { get; set; }
        public bool RewardsRefunded { get; set; }
        public bool RewardsSuspended { get; set; }
        public int RefundedAmount { get; set; }
        public DateTime? SuspensionUntil { get; set; }
    }
}
