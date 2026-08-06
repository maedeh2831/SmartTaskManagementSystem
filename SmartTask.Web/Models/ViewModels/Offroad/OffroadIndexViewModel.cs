using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartTask.Web.Models.ViewModels.Offroad;

public class OffroadIndexViewModel
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = null!;
    public List<OffroadTaskListItemViewModel> Tasks { get; set; } = new();
    public List<SelectListItem> ProjectMembers { get; set; } = new();
}