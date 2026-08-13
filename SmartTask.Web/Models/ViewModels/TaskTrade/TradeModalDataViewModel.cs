using Microsoft.AspNetCore.Mvc.Rendering;

namespace SmartTask.Web.Models.ViewModels.TaskTrade;

public class TradeModalDataViewModel
{
    public int TaskId { get; set; }
    public int ProjectId { get; set; }
    public List<SelectListItem> ProjectMembers { get; set; } = new();
}