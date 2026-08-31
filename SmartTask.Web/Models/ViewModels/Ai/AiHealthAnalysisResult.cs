namespace SmartTask.Web.Models.ViewModels.Ai;

/// <summary>
/// خروجی ساختاریافته LLM برای تحلیل سلامت پروژه.
/// LLM این مدل رو به صورت JSON برمی‌گردونه.
/// </summary>
public class AiHealthAnalysisResult
{
    /// <summary>ارزیابی کلی LLM از وضعیت پروژه</summary>
    public string OverallAssessment { get; set; } = string.Empty;

    /// <summary>بخش‌های بحرانی که نیاز به توجه فوری دارن</summary>
    public List<string> CriticalAreas { get; set; } = new();

    /// <summary>پیشنهادات عملی برای بهبود</summary>
    public List<string> Recommendations { get; set; } = new();

    /// <summary>پیش‌بینی LLM از آینده پروژه اگه تغییری ایجاد نشه</summary>
    public string Forecast { get; set; } = string.Empty;

    /// <summary>اولویت اقدامات پیشنهادی</summary>
    public List<string> ActionItems { get; set; } = new();
}
