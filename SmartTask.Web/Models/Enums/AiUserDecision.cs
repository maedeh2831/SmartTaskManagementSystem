namespace SmartTask.Web.Models.Enums;

/// <summary>
/// تصمیم مدیر در پاسخ به پیشنهاد هوش مصنوعی.
/// </summary>
public enum AiUserDecision
{
    /// <summary>مدیر پیشنهاد AI رو پذیرفت و اعمال کرد</summary>
    Accepted = 0,

    /// <summary>مدیر پیشنهاد AI رو رد کرد</summary>
    Rejected = 1,

    /// <summary>مدیر پیشنهاد AI رو بدون تغییر گذاشت (مشاهده کرد ولی تصمیم نگرفت)</summary>
    Ignored = 2
}
