using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;

namespace SmartTask.Web.Services.Interfaces;

public interface IOffroadTaskService : IBaseService<OffroadTask>
{
    Task<List<OffroadTask>> GetByProjectAsync(int projectId);
    Task<bool> CanManageOffroadTaskAsync(int offroadTaskId, int userId);
    Task ChangeStatusAsync(int id, OffroadStatusType status);
    Task ChangePriorityAsync(int id, OffroadPriorityType priority);
    Task AssignAsync(int id, int? userId);
    new Task DeleteAsync(int id);
}