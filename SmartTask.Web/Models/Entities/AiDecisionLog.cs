using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.Entities;

/// <summary>
/// ثبت تصمیمات مدیر در پاسخ به پیشنهادات هوش مصنوعی.
/// برای ارزیابی صحت AI و تحلیل عملکرد سیستم تصمیم‌یار استفاده میشه.
/// </summary>
public class AiDecisionLog : BaseEntity
{
    /// <summary>نوع موجودیت مورد نظر (Task, Project, Sprint)</summary>
    public AiDecisionEntityType EntityType { get; set; }

    /// <summary>شناسه موجودیت مورد نظر</summary>
    public int EntityId { get; set; }

    /// <summary>نوع تصمیم (Priority, Risk, Health, Workload)</summary>
    public AiDecisionType DecisionType { get; set; }

    /// <summary>پیشنهاد عددی AI (مثلاً امتیاز ریسک یا اولویت)</summary>
    public int? AiScore { get; set; }

    /// <summary>پیشنهاد متنی AI (مثلاً اولویت پیشنهادی یا تحلیل ریسک)</summary>
    public string? AiSuggestion { get; set; }

    /// <summary>لیست دلایل AI (JSON array)</summary>
    public string? AiReasons { get; set; }

    /// <summary>تصمیم مدیر: پذیرش یا رد</summary>
    public AiUserDecision UserDecision { get; set; }

    /// <summary>دلیل مدیر برای رد پیشنهاد (اختیاری)</summary>
    public string? UserReason { get; set; }

    /// <summary>شناسه کاربری که تصمیم گرفته</summary>
    public int UserId { get; set; }

    /// <summary>تاریخ تصمیم</summary>
    public DateTime DecisionDate { get; set; } = DateTime.Now;
}
