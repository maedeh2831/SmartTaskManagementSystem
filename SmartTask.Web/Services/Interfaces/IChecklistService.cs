using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Services.Interfaces;

public interface IChecklistService
{
    Task<List<Checklist>> GetByTaskAsync(int taskItemId);
    Task<Checklist> CreateChecklistAsync(int taskItemId, string title);
    Task<bool> CanManageChecklistAsync(int checklistId, int userId);
    Task DeleteChecklistAsync(int checklistId);
    Task<bool> CanManageItemAsync(int itemId, int userId);
    Task AddItemAsync(int checklistId, string title);
    Task ToggleItemAsync(int itemId);
    Task DeleteItemAsync(int itemId);
}