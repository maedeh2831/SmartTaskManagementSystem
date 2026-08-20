using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Backlog;

public class SprintGroupViewModel
{
    public int SprintId { get; set; }
    public string SprintName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public SprintStatusType Status { get; set; }
    public List<UserStoryListItemViewModel> Stories { get; set; } = new();
}
