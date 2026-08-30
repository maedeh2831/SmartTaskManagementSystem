using SmartTask.Web.Models.ViewModels.ProjectSimulation;

namespace SmartTask.Web.Services.Interfaces
{
    public interface ICriticalPathAnalyzer
    {
        /// <summary>
        /// Calculates the critical path for a project using Dijkstra's algorithm.
        /// The critical path is the longest sequence of dependent tasks that determines project duration.
        /// </summary>
        /// <param name="projectId">Project identifier</param>
        /// <returns>CriticalPathDto containing path, length, and slack times for all tasks</returns>
        /// <remarks>Performance target: less than 500ms for projects with less than 1000 tasks</remarks>
        Task<CriticalPathDto> CalculateCriticalPathAsync(int projectId);

        /// <summary>
        /// Gets all tasks that are on the critical path
        /// </summary>
        Task<List<int>> GetCriticalPathTasksAsync(int projectId);

        /// <summary>
        /// Calculates slack time for a specific task (how many days the task can be delayed without affecting project end date)
        /// </summary>
        Task<int> GetTaskSlackTimeAsync(int taskId);
    }
}
