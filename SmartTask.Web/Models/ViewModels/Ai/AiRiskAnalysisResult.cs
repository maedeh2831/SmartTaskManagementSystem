namespace SmartTask.Web.Models.ViewModels.Ai;

/// <summary>
/// خروجی ساختاریافته LLM برای تحلیل ریسک تأخیر پروژه.
/// LLM این مدل رو به صورت JSON برمی‌گردونه و سیستم Parse می‌کنه.
/// </summary>
public class AiRiskAnalysisResult
{
    /// <summary>امتیاز ریسک که LLM محاسبه کرده (0-100)</summary>
    public int RiskScore { get; set; }

    /// <summary>سطح ریسک</summary>
    public string RiskLevel { get; set; } = string.Empty;

    /// <summary>عوامل اصلی تأثیرگذار روی ریسک</summary>
    public List<string> Factors { get; set; } = new();

    /// <summary>پیشنهاد عملی برای کاهش ریسک</summary>
    public string Suggestion { get; set; } = string.Empty;

    /// <summary>سطح اطمینان LLM به تحلیل خودش</summary>
    public string Confidence { get; set; } = "medium";

    /// <summary>تحلیل کوتاه فارسی (1-2 جمله)</summary>
    public string Summary { get; set; } = string.Empty;
}
