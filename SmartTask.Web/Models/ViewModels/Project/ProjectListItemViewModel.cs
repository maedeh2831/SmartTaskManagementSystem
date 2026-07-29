using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Project;

public class ProjectListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Key { get; set; } = null!;
    public string? Description { get; set; }
    public string Color { get; set; } = "#4F46E5";
    public string Icon { get; set; } = "fa-solid fa-diagram-project";
    public ProjectStatusType Status { get; set; }
    public ProjectPriorityType Priority { get; set; }
    public DateTime? DueDate { get; set; }
    public bool IsArchived { get; set; }
    public int MembersCount { get; set; }
    public int TeamsCount { get; set; }
    public DateTime CreateDate { get; set; }
}