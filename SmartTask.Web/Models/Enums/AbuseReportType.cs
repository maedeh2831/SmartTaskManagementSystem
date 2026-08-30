/*
| Module      : Gamification
| Enum        : AbuseReportType
| Purpose     : انواع سوء استفاده قابل ردیابی
*/

namespace SmartTask.Web.Models.Enums
{
    public enum AbuseReportType
    {
        RapidCompletion = 0,          // بیش از 50 تسک در ساعت
        VelocityAnomaly = 1,           // تغییر غیرمنتظره در سرعت کسب تجربه
        DuplicateCompletions = 2,      // تکمیل تسک‌های تکراری
        SystemManipulation = 3,        // عدم تطابق تاریخ‌ها
        LowEstimateTaskFarming = 4,    // الگوی شمارش تسک‌های کم‌برآورد‌شده
        BulkAchievementUnlock = 5,     // باز شدن ناگهانی دستاورد‌های متعدد
        MarketplaceExploit = 6,        // بهره‌برداری از بازار
        SuspiciousPattern = 7          // الگوی مریب دیگر
    }
}
