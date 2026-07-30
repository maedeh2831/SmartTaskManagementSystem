using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Services.Interfaces;

public interface IUserStoryService : IBaseService<UserStory>
{
    Task<UserStory?> GetDetailsAsync(int id);
    Task<List<UserStory>> GetBacklogStoriesAsync(int projectId);
    Task<List<UserStory>> GetSprintStoriesAsync(int sprintId);
    Task<bool> ExistsByTitleAsync(
        int backlogId,
        string title,
        int? excludeId = null);
    Task<bool> CanManageBacklogAsync(int projectId, int userId);
    Task<bool> CanManageStoryAsync(int storyId, int userId);
    Task MoveToSprintAsync(int storyId, int sprintId);
    Task RemoveFromSprintAsync(int storyId);
    Task ChangePriorityAsync(int storyId, StoryPriorityType priority);
    Task ChangeStatusAsync(int storyId, StoryStatusType status);
    Task ChangeOwnerAsync(int storyId, int? ownerId);
    Task ReorderAsync(List<int> orderedIds);
    new Task DeleteAsync(int id);
}