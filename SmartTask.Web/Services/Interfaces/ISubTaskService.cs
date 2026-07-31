using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Services.Interfaces;

public interface ISubTaskService : IBaseService<SubTaskItem>
{
    Task<List<SubTaskItem>> GetByTaskAsync(int taskItemId);
    Task<bool> CanManageSubTaskAsync(int subTaskId, int userId);
    Task ToggleCompleteAsync(int subTaskId);
    new Task DeleteAsync(int id);
}