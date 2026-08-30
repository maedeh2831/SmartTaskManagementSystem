using SmartTask.Web.Models.ViewModels.ProjectSimulation;

namespace SmartTask.Web.Services.Interfaces
{
    public interface IProjectSimulationEngine
    {
        /// <summary>
        /// Initializes a simulation baseline for a project
        /// </summary>
        Task<int> CreateSimulationAsync(int projectId, string simulationName);

        /// <summary>
        /// Runs a what-if scenario: simulates delay for a task and stores the scenario
        /// </summary>
        Task<SimulationScenarioDto> RunScenarioAsync(int projectId, int taskId, int delayDays, string? scenarioName = null);

        /// <summary>
        /// Retrieves a stored scenario by ID
        /// </summary>
        Task<SimulationScenarioDto?> GetScenarioAsync(int scenarioId);

        /// <summary>
        /// Lists all scenarios for a project simulation
        /// </summary>
        Task<List<SimulationScenarioDto>> GetProjectScenariosAsync(int projectSimulationId);

        /// <summary>
        /// Compares two scenarios and returns metrics on which is better
        /// </summary>
        Task<ScenarioComparisonDto> CompariousScenariosAsync(int scenarioAId, int scenarioBId);

        /// <summary>
        /// Gets or creates a project simulation
        /// </summary>
        Task<int> GetOrCreateSimulationAsync(int projectId);
    }
}
