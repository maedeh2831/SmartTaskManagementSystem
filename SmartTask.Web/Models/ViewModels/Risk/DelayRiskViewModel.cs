namespace SmartTask.Web.Models.ViewModels.Risk;

public class DelayRiskViewModel
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = null!;

    public int RiskScore { get; set; }
    public string RiskLevel { get; set; } = "low"; // low, medium, high, critical
    public string RiskLevelDisplay { get; set; } = null!;

    public int OverdueTasksCount { get; set; }
    public int TotalOpenTasksCount { get; set; }
    public int OverdueScore { get; set; }

    public int OverloadedMembersCount { get; set; }
    public int TotalMembersCount { get; set; }
    public int WorkloadScore { get; set; }

    public int RiskyDependencyChainsCount { get; set; }
    public int DependencyScore { get; set; }

    public int RecentCascadeCount { get; set; }
    public int CascadeScore { get; set; }

    // ===== فیلدهای تحلیل AI =====
    /// <summary>تحلیل متنی هوش مصنوعی (عوامل + پیشنهاد)</summary>
    public string? AiAnalysis { get; set; }

    /// <summary>عوامل تأثیرگذار از دید AI</summary>
    public List<string> AiFactors { get; set; } = new();

    /// <summary>پیشنهاد عملی AI</summary>
    public string? AiSuggestion { get; set; }

    /// <summary>امتیاز ریسک از دید AI (ممکنه با الگوریتم فرق کنه)</summary>
    public int? AiRiskScore { get; set; }

    /// <summary>سطح اطمینان AI به تحلیلش</summary>
    public string? AiConfidence { get; set; }
}