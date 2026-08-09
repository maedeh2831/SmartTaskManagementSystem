using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.TaskTrade;

public class TaskTradeItemViewModel
{
    public int Id { get; set; }
    public string RequesterName { get; set; } = null!;
    public string TargetName { get; set; } = null!;
    public string RequesterTaskTitle { get; set; } = null!;
    public string? TargetTaskTitle { get; set; }
    public string? Message { get; set; }
    public TradeRequestStatusType Status { get; set; }
    public DateTime CreateDate { get; set; }
    public bool IsIncoming { get; set; }
}