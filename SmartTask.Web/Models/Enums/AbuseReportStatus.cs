/*
| Module      : Gamification
| Enum        : AbuseReportStatus
| Purpose     : وضعیت‌های گزارش سوء استفاده
*/

namespace SmartTask.Web.Models.Enums
{
    public enum AbuseReportStatus
    {
        Pending = 0,         // در انتظار بررسی
        UnderReview = 1,     // در حال بررسی
        Confirmed = 2,       // تأیید شده
        False = 3,           // تأیید نشده
        Resolved = 4,        // حل شده
        Dismissed = 5        // رد شده
    }
}
