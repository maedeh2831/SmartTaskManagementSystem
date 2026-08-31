/*
| Module      : Simulation
| Entity      : SimulationScenario
| Purpose     : Stores what-if scenario parameters and results for project impact analysis.
*/

namespace SmartTask.Web.Models.Entities
{
    public class SimulationScenario : BaseEntity
    {
        public int ProjectSimulationId { get; set; }

        public int SimulatedTaskId { get; set; }

        public string ScenarioName { get; set; } = null!;

        public string? Description { get; set; }

        /// <summary>
        /// Original task end date before delay
        /// </summary>
        public DateTime OriginalTaskEndDate { get; set; }

        /// <summary>
        /// Simulated delay in days
        /// </summary>
        public int DelayDays { get; set; }

        /// <summary>
        /// New project end date after simulation
        /// </summary>
        public DateTime NewProjectEndDate { get; set; }

        /// <summary>
        /// Original project end date before simulation
        /// </summary>
        public DateTime OriginalProjectEndDate { get; set; }

        /// <summary>
        /// Total project delay caused by this scenario (days)
        /// </summary>
        public int ProjectDelayDays { get; set; }

        /// <summary>
        /// Number of tasks affected by this delay
        /// </summary>
        public int TotalAffectedTasks { get; set; }

        /// <summary>
        /// Serialized JSON of affected task details
        /// </summary>
        public string? AffectedTasksJson { get; set; }

        /// <summary>
        /// Serialized JSON of critical path details
        /// </summary>
        public string? CriticalPathJson { get; set; }

        /// <summary>
        /// Risk level: Low, Medium, High
        /// </summary>
        public string RiskLevel { get; set; } = "Medium";

        public DateTime SimulatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ProjectSimulation ProjectSimulation { get; set; } = null!;
    }
}
