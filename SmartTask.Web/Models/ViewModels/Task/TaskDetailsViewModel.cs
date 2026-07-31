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

    // Story 2: SubTasks
    public List<SubTaskItemViewModel> SubTasks { get; set; } = new();
    public int SubTasksTotal => SubTasks.Count;
    public int SubTasksCompleted => SubTasks.Count(x => x.IsCompleted);
    public int SubTasksProgressPercent =>
        SubTasksTotal == 0 ? 0 : (int)Math.Round((double)SubTasksCompleted / SubTasksTotal * 100);

    // Story 3: Assignees
    public List<AssigneeOptionViewModel> Assignees { get; set; } = new();
    public List<AssigneeOptionViewModel> AvailableMembers { get; set; } = new();
}