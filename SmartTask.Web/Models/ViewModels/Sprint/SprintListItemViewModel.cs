using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Sprint;

public class SprintListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Goal { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Capacity { get; set; }
    public SprintStatusType Status { get; set; }
    public int UserStoriesCount { get; set; }

    public int TotalDays => Math.Max(1, (EndDate - StartDate).Days);

    public int ElapsedDays =>
        Math.Clamp((DateTime.Today - StartDate).Days, 0, TotalDays);

    public int ProgressPercent =>
        (int)Math.Round((double)ElapsedDays / TotalDays * 100);
}