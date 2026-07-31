using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Sprint;

public class SprintPlanningViewModel
{
    public int SprintId { get; set; }
    public string SprintName { get; set; } = null!;
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = null!;
    public int Capacity { get; set; }
    public bool CanManage { get; set; }
    public List<PlanningStoryItemViewModel> BacklogStories { get; set; } = new();
    public List<PlanningStoryItemViewModel> SprintStories { get; set; } = new();
    public int PlannedPoints => SprintStories.Sum(x => x.StoryPoint);
    public int CapacityPercent => Capacity <= 0 ? 0 : (int)Math.Round((double)PlannedPoints / Capacity * 100);
}