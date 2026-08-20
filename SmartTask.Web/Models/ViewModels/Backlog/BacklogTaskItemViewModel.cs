using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Backlog;

public class BacklogTaskItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public TaskStatusType Status { get; set; }
    public TaskPriorityType Priority { get; set; }
    public TaskType Type { get; set; }
    public int Estimate { get; set; }
    public DateTime? DueDate { get; set; }
    public string? AssigneeName { get; set; }
}
