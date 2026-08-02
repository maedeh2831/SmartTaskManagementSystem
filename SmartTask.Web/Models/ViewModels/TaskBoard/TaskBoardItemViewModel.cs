using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.TaskBoard;

public class TaskBoardItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public TaskStatusType Status { get; set; }
    public TaskPriorityType Priority { get; set; }
    public TaskType Type { get; set; }
    public int Estimate { get; set; }
    public DateTime? DueDate { get; set; }
    public int UserStoryId { get; set; }
    public string UserStoryTitle { get; set; } = null!;
    public List<string> AssigneeNames { get; set; } = new();
    public List<BoardLabelBadgeViewModel> Labels { get; set; } = new();
}