using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Task;

public class TaskDetailsViewModel
{
    public int Id { get; set; }
    public int UserStoryId { get; set; }
    public string UserStoryTitle { get; set; } = null!;
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public TaskStatusType Status { get; set; }
    public TaskPriorityType Priority { get; set; }
    public TaskType Type { get; set; }
    public int Estimate { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public DateTime CreateDate { get; set; }

    public bool CanManage { get; set; }

    public List<SubTaskItemViewModel> SubTasks { get; set; } = new();
    public int SubTasksTotal => SubTasks.Count;
    public int SubTasksCompleted => SubTasks.Count(x => x.IsCompleted);
    public int SubTasksProgressPercent =>
        SubTasksTotal == 0 ? 0 : (int)Math.Round((double)SubTasksCompleted / SubTasksTotal * 100);

    public List<AssigneeOptionViewModel> Assignees { get; set; } = new();
    public List<AssigneeOptionViewModel> AvailableMembers { get; set; } = new();

    public List<CommentViewModel> Comments { get; set; } = new();
    public List<AttachmentViewModel> Attachments { get; set; } = new();
    public List<LabelBadgeViewModel> Labels { get; set; } = new();
    public List<LabelBadgeViewModel> AvailableLabels { get; set; } = new();
    public List<ChecklistViewModel> Checklists { get; set; } = new();
    public List<TimeLogItemViewModel> TimeLogs { get; set; } = new();
    public ActiveTimerViewModel? MyActiveTimer { get; set; }
    public int TotalLoggedMinutes { get; set; }
    public string TotalLoggedDisplay =>
        TotalLoggedMinutes >= 60
            ? $"{TotalLoggedMinutes / 60} ساعت و {TotalLoggedMinutes % 60} دقیقه"
            : $"{TotalLoggedMinutes} دقیقه";

    public SmartTask.Web.Models.ViewModels.Dependency.TaskDependencyWidgetViewModel Dependency { get; set; } = null!;
    public List<SmartTask.Web.Models.ViewModels.Dependency.CascadeInfoViewModel> CascadeInfo { get; set; } = new();
    public SmartTask.Web.Models.ViewModels.Priority.SmartPriorityViewModel SmartPriority { get; set; } = null!;
}