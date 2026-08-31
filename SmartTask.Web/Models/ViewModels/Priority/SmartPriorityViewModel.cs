using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Priority;

public class SmartPriorityViewModel
{
    public int TaskId { get; set; }
    public TaskPriorityType CurrentPriority { get; set; }
    public TaskPriorityType SuggestedPriority { get; set; }
    public int TotalScore { get; set; }
    public int UrgencyScore { get; set; }
    public int DependencyScore { get; set; }
    public int WorkloadScore { get; set; }
    public List<string> Reasons { get; set; } = new();
    public bool IsDifferent => CurrentPriority != SuggestedPriority;
    public bool CanApply { get; set; }

    // ===== فیلدهای تحلیل AI =====
    /// <summary>دلایل تکمیلی از دید هوش مصنوعی</summary>
    public List<string> AiReasons { get; set; } = new();

    /// <summary>عمل پیشنهادی AI</summary>
    public string? AiSuggestedAction { get; set; }

    /// <summary>توضیح AI چرا این اولویت پیشنهاد شده</summary>
    public string? AiExplanation { get; set; }

    /// <summary>پیشنهاد AI در مورد تخصیص منابع</summary>
    public string? AiResourceSuggestion { get; set; }
}