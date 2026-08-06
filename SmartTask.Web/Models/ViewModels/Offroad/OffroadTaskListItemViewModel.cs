using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Offroad;

public class OffroadTaskListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public OffroadStatusType Status { get; set; }
    public OffroadPriorityType Priority { get; set; }
    public string CreatedByName { get; set; } = null!;
    public string? AssignedToName { get; set; }
    public int? AssignedToUserId { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime CreateDate { get; set; }
    public bool CanManage { get; set; }
}