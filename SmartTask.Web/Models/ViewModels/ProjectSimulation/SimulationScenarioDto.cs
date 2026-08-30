using System.Text.Json.Serialization;

namespace SmartTask.Web.Models.ViewModels.ProjectSimulation
{
    public class SimulationScenarioDto
    {
        public int Id { get; set; }

        public int ProjectSimulationId { get; set; }

        public string ScenarioName { get; set; } = null!;

        public string? Description { get; set; }

        public int SimulatedTaskId { get; set; }

        public string SimulatedTaskTitle { get; set; } = null!;

        public int DelayDays { get; set; }

        public DateTime OriginalTaskEndDate { get; set; }

        public DateTime OriginalProjectEndDate { get; set; }

        public DateTime NewProjectEndDate { get; set; }

        public int ProjectDelayDays { get; set; }

        public int TotalAffectedTasks { get; set; }

        public string RiskLevel { get; set; } = "Medium";

        public DateTime SimulatedAt { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public ImpactAnalysisDto? ImpactAnalysis { get; set; }
    }

    public class CreateSimulationScenarioRequest
    {
        public int ProjectId { get; set; }

        public int TaskId { get; set; }

        public int DelayDays { get; set; }

        public string? ScenarioName { get; set; }

        public string? Description { get; set; }
    }

    public class ScenarioComparisonDto
    {
        public SimulationScenarioDto ScenarioA { get; set; } = null!;

        public SimulationScenarioDto ScenarioB { get; set; } = null!;

        public ComparisonMetricsDto Metrics { get; set; } = null!;
    }

    public class ComparisonMetricsDto
    {
        public int ProjectDelayDifference { get; set; }

        public int AffectedTasksDifference { get; set; }

        public string BetterScenario { get; set; } = null!; // "A" or "B"

        public decimal ImpactReductionPercentage { get; set; }
    }
}
