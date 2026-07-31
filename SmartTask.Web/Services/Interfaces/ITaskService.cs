using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Services.Interfaces
{
    public interface ITaskService : IBaseService<TaskItem>
    {
        Task<TaskItem?> GetDetailsAsync(int id);
        Task<List<TaskItem>> GetByUserStoryAsync(int userStoryId);
        Task<bool> ExistsByTitleAsync(int userStoryId, string title, int? excludeId = null);
        Task<bool> CanManageTaskAsync(int taskId, int userId);
        Task ChangeStatusAsync(int taskId, TaskStatusType status);
        new Task DeleteAsync(int id);
    }
}