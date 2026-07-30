using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.Backlog;

public class UserStoryListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public int StoryPoint { get; set; }
    public int BusinessValue { get; set; }
    public int Order { get; set; }
    public StoryPriorityType Priority { get; set; }
    public StoryStatusType Status { get; set; }
    public int? OwnerId { get; set; }
    public string? OwnerName { get; set; }
}