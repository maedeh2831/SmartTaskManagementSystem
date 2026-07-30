namespace SmartTask.Web.Models.ViewModels.Sprint;

public class SprintIndexViewModel
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = null!;
    public bool CanManageSprints { get; set; }
    public List<SprintListItemViewModel> Sprints { get; set; } = new();
}