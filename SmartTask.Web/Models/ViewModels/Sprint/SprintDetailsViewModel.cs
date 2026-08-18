using SmartTask.Web.Models.Enums;
namespace SmartTask.Web.Models.ViewModels.Sprint;
public class SprintDetailsViewModel
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Goal { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Capacity { get; set; }
    public SprintStatusType Status { get; set; }
    public DateTime CreateDate { get; set; }
    public bool CanManage { get; set; }
    public List<PlanningStoryItemViewModel> Stories { get; set; } = new();
    public int TotalDays => Math.Max(1, (EndDate - StartDate).Days);
    public int ElapsedDays =>
        Math.Clamp((DateTime.Today - StartDate).Days, 0, TotalDays);
    public int ProgressPercent =>
        (int)Math.Round((double)ElapsedDays / TotalDays * 100);
}