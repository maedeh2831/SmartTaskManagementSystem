using SmartTask.Web.Models.Entities;

namespace SmartTask.Web.Services.Interfaces;

public interface ITaskLabelService
{
    Task<List<Label>> GetLabelsForTaskAsync(int taskItemId);
    Task AssignLabelAsync(int taskItemId, int labelId);
    Task RemoveLabelAsync(int taskItemId, int labelId);
}