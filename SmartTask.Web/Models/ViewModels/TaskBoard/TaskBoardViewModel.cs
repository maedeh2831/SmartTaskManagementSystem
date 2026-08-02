using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.TaskBoard;

public class TaskBoardViewModel
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = null!;
    public bool CanManage { get; set; }

    public List<TaskBoardItemViewModel> Tasks { get; set; } = new();

    // فیلترهای فعلی (برای نگه‌داشتن انتخاب کاربر بعد از submit)
    public int? SelectedAssigneeId { get; set; }
    public TaskPriorityType? SelectedPriority { get; set; }
    public TaskType? SelectedType { get; set; }
    public int? SelectedLabelId { get; set; }

    // گزینه‌های فیلتر
    public List<BoardFilterOptionViewModel> AvailableAssignees { get; set; } = new();
    public List<BoardFilterOptionViewModel> AvailableLabels { get; set; } = new();

    public bool HasActiveFilters =>
        SelectedAssigneeId.HasValue || SelectedPriority.HasValue ||
        SelectedType.HasValue || SelectedLabelId.HasValue;
}