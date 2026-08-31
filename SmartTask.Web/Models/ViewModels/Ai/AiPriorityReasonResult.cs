namespace SmartTask.Web.Models.ViewModels.Ai;

/// <summary>
/// خروجی ساختاریافته LLM برای دلایل هوشمند اولویت‌بندی.
/// LLM این مدل رو به صورت JSON برمی‌گردونه.
/// </summary>
public class AiPriorityReasonResult
{
    /// <summary>دلایل تکمیلی LLM برای اولویت پیشنهادی</summary>
    public List<string> AiReasons { get; set; } = new();

    /// <summary>عمل پیشنهادی LLM</summary>
    public string AiSuggestedAction { get; set; } = string.Empty;

    /// <summary>توضیح کوتاه چرا این اولویت پیشنهاد شده</summary>
    public string Explanation { get; set; } = string.Empty;

    /// <summary>پیشنهاد LLM در مورد تخصیص منابع</summary>
    public string ResourceSuggestion { get; set; } = string.Empty;
}
