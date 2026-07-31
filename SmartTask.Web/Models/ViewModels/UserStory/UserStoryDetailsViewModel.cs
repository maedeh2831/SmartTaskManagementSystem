using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Models.ViewModels.UserStory;

public class UserStoryDetailsViewModel
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public string? AcceptanceCriteria { get; set; }
    public int StoryPoint { get; set; }
    public int BusinessValue { get; set; }
    public StoryPriorityType Priority { get; set; }
    public StoryStatusType Status { get; set; }
    public string? SprintName { get; set; }
    public string? OwnerName { get; set; }
    public DateTime CreateDate { get; set; }
    public bool CanManage { get; set; }
}