namespace SmartTask.Web.Models.Enums;

/// <summary>
/// نوع موجودیتی که تصمیم AI در مورد اون گرفته شده.
/// </summary>
public enum AiDecisionEntityType
{
    /// <summary>تصمیم در مورد اولویت یا تحلیل یک Task</summary>
    Task = 0,

    /// <summary>تصمیم در مورد ریسک یا سلامت یک Project</summary>
    Project = 1,

    /// <summary>تصمیم در مورد گزارش یک Sprint</summary>
    Sprint = 2
}
