/*
| Module      : Simulation
| Entity      : ProjectSimulation
| Purpose     : Stores project simulation scenarios for what-if analysis and impact modeling.
*/

namespace SmartTask.Web.Models.Entities
{
    public class ProjectSimulation : BaseEntity
    {
        public int ProjectId { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public DateTime BaselineStartDate { get; set; }

        public DateTime BaselineEndDate { get; set; }

        public int TotalTasksCount { get; set; }

        public DateTime? CriticalPathCalculatedAt { get; set; }

        public int CriticalPathLengthDays { get; set; }

        // Navigation Properties
        public Project Project { get; set; } = null!;

        public ICollection<SimulationScenario> Scenarios { get; set; } = new List<SimulationScenario>();
    }
}
