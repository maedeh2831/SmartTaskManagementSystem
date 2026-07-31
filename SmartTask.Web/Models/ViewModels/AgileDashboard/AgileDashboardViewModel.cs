using SmartTask.Web.Models.DTOs;
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.AgileDashboard;

public class AgileDashboardViewModel
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = null!;

    public int SprintId { get; set; }
    public string SprintName { get; set; } = null!;
    public SprintStatusType SprintStatus { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Capacity { get; set; }

    public int TotalStories { get; set; }
    public int DoneStories { get; set; }
    public int InProgressStories { get; set; }
    public int TodoStories { get; set; }

    public int TotalPoints { get; set; }
    public int DonePoints { get; set; }

    public List<BurndownPointDto> BurndownPoints { get; set; } = new();
    public List<VelocityPointDto> VelocityPoints { get; set; } = new();
    public List<SprintOptionViewModel> AvailableSprints { get; set; } = new();

    public int TotalDays => Math.Max(1, (EndDate.Date - StartDate.Date).Days);
    public int ElapsedDays => Math.Clamp((DateTime.Today - StartDate.Date).Days, 0, TotalDays);
    public int TimeProgressPercent => (int)Math.Round((double)ElapsedDays / TotalDays * 100);
    public int WorkProgressPercent => TotalPoints == 0 ? 0 : (int)Math.Round((double)DonePoints / TotalPoints * 100);
    public int RemainingDays => Math.Max(0, (EndDate.Date - DateTime.Today).Days);
}