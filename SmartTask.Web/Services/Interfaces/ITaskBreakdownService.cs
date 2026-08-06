namespace SmartTask.Web.Services.Interfaces;

public interface ITaskBreakdownService
{
    Task<List<string>> GenerateSubTasksAsync(int taskId);
}