namespace SmartTask.Web.Models.Enums;

/// <summary>
/// نوع تحلیل و پیشنهادی که AI ارائه کرده.
/// </summary>
public enum AiDecisionType
{
    /// <summary>پیشنهاد اولویت هوشمند</summary>
    Priority = 0,

    /// <summary>تحلیل ریسک تأخیر</summary>
    Risk = 1,

    /// <summary>تحلیل سلامت پروژه</summary>
    Health = 2,

    /// <summary>تحلیل بارکاری</summary>
    Workload = 3,

    /// <summary>گزارش اسپرینت</summary>
    SprintReport = 4,

    /// <summary>تجزیه وظیفه</summary>
    TaskBreakdown = 5
}
