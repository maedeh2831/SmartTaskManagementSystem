namespace SmartTask.Web.Models.ViewModels.Label;

public class LabelIndexViewModel
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = null!;
    public bool CanManage { get; set; }
    public List<LabelListItemViewModel> Labels { get; set; } = new();
}