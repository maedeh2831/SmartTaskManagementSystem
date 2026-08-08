using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Dependency;

public class DependencyItemViewModel
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public string TaskTitle { get; set; } = null!;
    public TaskStatusType TaskStatus { get; set; }
    public bool IsRequired { get; set; }
}