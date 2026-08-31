namespace SmartTask.Web.Models.ViewModels.ProjectSimulation
{
    public class ImpactAnalysisDto
    {
        public int DelayedTaskId { get; set; }

        public string DelayedTaskTitle { get; set; } = null!;

        public int DelayDays { get; set; }

        public DateTime OriginalProjectEndDate { get; set; }

        public DateTime NewProjectEndDate { get; set; }

        public int ProjectDelayDays { get; set; }

        public int TotalAffectedTasks { get; set; }

        public string RiskLevel { get; set; } = "Medium"; // Low, Medium, High

        public List<AffectedTaskDto> AffectedTasks { get; set; } = new();

        public List<RippleEffectDto> RippleEffects { get; set; } = new();

        public DateTime AnalysisGeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class AffectedTaskDto
    {
        public int TaskId { get; set; }

        public string TaskTitle { get; set; } = null!;

        public DateTime OriginalEndDate { get; set; }

        public DateTime NewEndDate { get; set; }

        public int DaysShifted { get; set; }

        public string DependencyPath { get; set; } = null!; // "Task A -> Task B -> Task C"

        public int DepthInDependencyChain { get; set; }
    }

    public class RippleEffectDto
    {
        public int TaskId { get; set; }

        public string TaskTitle { get; set; } = null!;

        public int DirectDependenciesAffected { get; set; }

        public int IndirectDependenciesAffected { get; set; }

        public int TotalDownstreamTasks { get; set; }

        public string SeverityLevel { get; set; } = "Medium";
    }
}
