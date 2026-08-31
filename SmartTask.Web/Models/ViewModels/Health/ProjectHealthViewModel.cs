namespace SmartTask.Web.Models.ViewModels.Health;

public class ProjectHealthViewModel
{
    public int ProjectId { get; set; }

    public int HealthScore { get; set; }
    public string HealthLevel { get; set; } = "good"; // excellent, good, fair, poor
    public string HealthLevelDisplay { get; set; } = null!;
    public string HealthIcon { get; set; } = null!;

    public int ScheduleHealth { get; set; }
    public int WorkloadHealth { get; set; }
    public int DependencyHealth { get; set; }
    public int DeliveryHealth { get; set; }

    public int CompletedTasksCount { get; set; }
    public int TotalTasksCount { get; set; }

    // ===== فیلدهای تحلیل AI =====
    /// <summary>ارزیابی کلی هوش مصنوعی از وضعیت پروژه</summary>
    public string? AiOverallAssessment { get; set; }

    /// <summary>بخش‌های بحرانی از دید AI</summary>
    public List<string> AiCriticalAreas { get; set; } = new();

    /// <summary>پیشنهادات عملی AI</summary>
    public List<string> AiRecommendations { get; set; } = new();

    /// <summary>پیش‌بینی AI از آینده پروژه</summary>
    public string? AiForecast { get; set; }

    /// <summary>اولویت اقدامات پیشنهادی AI</summary>
    public List<string> AiActionItems { get; set; } = new();
}