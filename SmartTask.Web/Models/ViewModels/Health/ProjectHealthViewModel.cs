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
}