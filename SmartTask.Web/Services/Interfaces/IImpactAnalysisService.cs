using SmartTask.Web.Models.ViewModels.ProjectSimulation;

namespace SmartTask.Web.Services.Interfaces
{
    public interface IImpactAnalysisService
    {
        /// <summary>
        /// Analyzes the ripple effects of delaying a specific task.
        /// Uses DFS to identify all downstream dependencies and calculates new end dates.
        /// </summary>
        /// <param name="projectId">Project identifier</param>
        /// <param name="taskId">Task to simulate delay for</param>
        /// <param name="delayDays">Number of days to delay the task</param>
        /// <returns>ImpactAnalysisDto containing affected tasks and ripple effects</returns>
        Task<ImpactAnalysisDto> AnalyzeImpactAsync(int projectId, int taskId, int delayDays);

        /// <summary>
        /// Finds all downstream tasks that depend on a given task (directly or indirectly)
        /// </summary>
        Task<List<int>> GetDownstreamTasksAsync(int taskId);

        /// <summary>
        /// Calculates new end dates for all affected tasks based on a delay
        /// </summary>
        Task<Dictionary<int, DateTime>> CalculateNewEndDatesAsync(List<int> affectedTaskIds, int delayDays);

        /// <summary>
        /// Determines risk level based on number of affected tasks and delay days
        /// </summary>
        string CalculateRiskLevel(int affectedTasksCount, int delayDays, int projectCriticalPathLength);
    }
}
