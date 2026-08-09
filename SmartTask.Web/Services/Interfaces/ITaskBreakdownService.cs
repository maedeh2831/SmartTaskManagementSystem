namespace SmartTask.Web.Services.AI;

public interface ITaskBreakdownService
{
    Task<List<string>> GenerateSubTasksAsync(int taskId, CancellationToken cancellationToken = default);
}