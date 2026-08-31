using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.ViewModels.ProjectSimulation;
using SmartTask.Web.Services.Interfaces;
using System.Text.Json;

namespace SmartTask.Web.Services.Implementations
{
    public class ProjectSimulationEngine : IProjectSimulationEngine
    {
        private readonly ApplicationDbContext _context;
        private readonly ICriticalPathAnalyzer _criticalPathAnalyzer;
        private readonly IImpactAnalysisService _impactAnalysisService;
        private readonly ILogger<ProjectSimulationEngine> _logger;

        public ProjectSimulationEngine(
            ApplicationDbContext context,
            ICriticalPathAnalyzer criticalPathAnalyzer,
            IImpactAnalysisService impactAnalysisService,
            ILogger<ProjectSimulationEngine> logger)
        {
            _context = context;
            _criticalPathAnalyzer = criticalPathAnalyzer;
            _impactAnalysisService = impactAnalysisService;
            _logger = logger;
        }

        public async Task<int> CreateSimulationAsync(int projectId, string simulationName)
        {
            var project = await _context.Projects
                .Include(p => p.UserStories)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                throw new ArgumentException($"Project {projectId} not found");

            // Calculate critical path for baseline
            var criticalPath = await _criticalPathAnalyzer.CalculateCriticalPathAsync(projectId);

            var userStoryCount = project.UserStories.Count(us => us.ViewState);
            var taskCount = await _context.TaskItems
                .CountAsync(t => project.UserStories.Select(us => us.Id).Contains(t.UserStoryId) && t.ViewState);

            var simulation = new ProjectSimulation
            {
                ProjectId = projectId,
                Name = simulationName,
                Description = $"Baseline simulation for {project.Name}",
                BaselineStartDate = project.StartDate ?? DateTime.Now,
                BaselineEndDate = criticalPath.ProjectEndDate,
                TotalTasksCount = taskCount,
                CriticalPathCalculatedAt = DateTime.UtcNow,
                CriticalPathLengthDays = criticalPath.CriticalPathLengthDays,
                CreatedDate = DateTime.UtcNow,
                ViewState = true
            };

            _context.ProjectSimulations.Add(simulation);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                $"Created simulation {simulation.Id} for project {projectId}. " +
                $"Critical path: {criticalPath.CriticalPathLengthDays}d, {taskCount} tasks");

            return simulation.Id;
        }

        public async Task<SimulationScenarioDto> RunScenarioAsync(int projectId, int taskId, int delayDays, string? scenarioName = null)
        {
            // Get or create simulation
            var simulationId = await GetOrCreateSimulationAsync(projectId);
            var simulation = await _context.ProjectSimulations
                .FirstOrDefaultAsync(s => s.Id == simulationId);

            if (simulation == null)
                throw new ArgumentException($"Simulation {simulationId} not found");

            var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId);
            if (task == null)
                throw new ArgumentException($"Task {taskId} not found");

            // Run impact analysis
            var impactAnalysis = await _impactAnalysisService.AnalyzeImpactAsync(projectId, taskId, delayDays);

            // Create scenario entity
            var scenario = new SimulationScenario
            {
                ProjectSimulationId = simulationId,
                SimulatedTaskId = taskId,
                ScenarioName = scenarioName ?? $"Delay {task.Title} by {delayDays}d",
                Description = $"Simulates delaying task '{task.Title}' by {delayDays} days",
                OriginalTaskEndDate = task.DueDate ?? DateTime.Now.AddDays(task.Estimate),
                DelayDays = delayDays,
                OriginalProjectEndDate = simulation.BaselineEndDate,
                NewProjectEndDate = impactAnalysis.NewProjectEndDate,
                ProjectDelayDays = impactAnalysis.ProjectDelayDays,
                TotalAffectedTasks = impactAnalysis.TotalAffectedTasks,
                RiskLevel = impactAnalysis.RiskLevel,
                AffectedTasksJson = JsonSerializer.Serialize(impactAnalysis.AffectedTasks),
                CriticalPathJson = JsonSerializer.Serialize(impactAnalysis.RippleEffects),
                SimulatedAt = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                ViewState = true
            };

            _context.SimulationScenarios.Add(scenario);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                $"Created scenario {scenario.Id}: Task {taskId} delayed by {delayDays}d, " +
                $"affecting {impactAnalysis.TotalAffectedTasks} tasks, project delay: {impactAnalysis.ProjectDelayDays}d");

            return MapToDto(scenario, task, impactAnalysis);
        }

        public async Task<SimulationScenarioDto?> GetScenarioAsync(int scenarioId)
        {
            var scenario = await _context.SimulationScenarios
                .FirstOrDefaultAsync(s => s.Id == scenarioId && s.ViewState);

            if (scenario == null)
                return null;

            var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == scenario.SimulatedTaskId);
            if (task == null)
                return null;

            ImpactAnalysisDto? impact = null;
            if (!string.IsNullOrEmpty(scenario.AffectedTasksJson))
            {
                try
                {
                    var affectedTasks = JsonSerializer.Deserialize<List<AffectedTaskDto>>(scenario.AffectedTasksJson);
                    var rippleEffects = !string.IsNullOrEmpty(scenario.CriticalPathJson)
                        ? JsonSerializer.Deserialize<List<RippleEffectDto>>(scenario.CriticalPathJson)
                        : new List<RippleEffectDto>();

                    impact = new ImpactAnalysisDto
                    {
                        DelayedTaskId = scenario.SimulatedTaskId,
                        DelayedTaskTitle = task.Title,
                        DelayDays = scenario.DelayDays,
                        OriginalProjectEndDate = scenario.OriginalProjectEndDate,
                        NewProjectEndDate = scenario.NewProjectEndDate,
                        ProjectDelayDays = scenario.ProjectDelayDays,
                        TotalAffectedTasks = scenario.TotalAffectedTasks,
                        RiskLevel = scenario.RiskLevel,
                        AffectedTasks = affectedTasks ?? new List<AffectedTaskDto>(),
                        RippleEffects = rippleEffects ?? new List<RippleEffectDto>()
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error deserializing impact data for scenario {scenarioId}");
                }
            }

            return MapToDto(scenario, task, impact);
        }

        public async Task<List<SimulationScenarioDto>> GetProjectScenariosAsync(int projectSimulationId)
        {
            var scenarios = await _context.SimulationScenarios
                .Where(s => s.ProjectSimulationId == projectSimulationId && s.ViewState)
                .OrderByDescending(s => s.SimulatedAt)
                .ToListAsync();

            var result = new List<SimulationScenarioDto>();

            foreach (var scenario in scenarios)
            {
                var task = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == scenario.SimulatedTaskId);
                if (task != null)
                    result.Add(MapToDto(scenario, task, null));
            }

            return result;
        }

        public async Task<ScenarioComparisonDto> CompariousScenariosAsync(int scenarioAId, int scenarioBId)
        {
            var scenarioA = await _context.SimulationScenarios
                .FirstOrDefaultAsync(s => s.Id == scenarioAId && s.ViewState);
            var scenarioB = await _context.SimulationScenarios
                .FirstOrDefaultAsync(s => s.Id == scenarioBId && s.ViewState);

            if (scenarioA == null || scenarioB == null)
                throw new ArgumentException("One or both scenarios not found");

            var taskA = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == scenarioA.SimulatedTaskId);
            var taskB = await _context.TaskItems.FirstOrDefaultAsync(t => t.Id == scenarioB.SimulatedTaskId);

            if (taskA == null || taskB == null)
                throw new ArgumentException("Task for scenario not found");

            var dtoA = MapToDto(scenarioA, taskA, null);
            var dtoB = MapToDto(scenarioB, taskB, null);

            var projectDelayDiff = scenarioB.ProjectDelayDays - scenarioA.ProjectDelayDays;
            var affectedTasksDiff = scenarioB.TotalAffectedTasks - scenarioA.TotalAffectedTasks;
            var betterScenario = projectDelayDiff > 0 ? "A" : (projectDelayDiff < 0 ? "B" : "Equal");

            var impactReductionPercent = scenarioA.ProjectDelayDays > 0
                ? (decimal)Math.Abs(projectDelayDiff) / scenarioA.ProjectDelayDays * 100
                : 0;

            _logger.LogInformation(
                $"Scenario comparison: A ({scenarioA.Id}) vs B ({scenarioB.Id}). " +
                $"Delay difference: {projectDelayDiff}d, affected tasks diff: {affectedTasksDiff}");

            return new ScenarioComparisonDto
            {
                ScenarioA = dtoA,
                ScenarioB = dtoB,
                Metrics = new ComparisonMetricsDto
                {
                    ProjectDelayDifference = Math.Abs(projectDelayDiff),
                    AffectedTasksDifference = Math.Abs(affectedTasksDiff),
                    BetterScenario = betterScenario,
                    ImpactReductionPercentage = impactReductionPercent
                }
            };
        }

        public async Task<int> GetOrCreateSimulationAsync(int projectId)
        {
            var existing = await _context.ProjectSimulations
                .Where(s => s.ProjectId == projectId && s.ViewState)
                .OrderByDescending(s => s.CreatedDate)
                .FirstOrDefaultAsync();

            if (existing != null)
                return existing.Id;

            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
            var simulationName = $"{project?.Name ?? "Project"} Simulation - {DateTime.Now:yyyy-MM-dd}";
            return await CreateSimulationAsync(projectId, simulationName);
        }

        private SimulationScenarioDto MapToDto(SimulationScenario scenario, TaskItem task, ImpactAnalysisDto? impact)
        {
            return new SimulationScenarioDto
            {
                Id = scenario.Id,
                ProjectSimulationId = scenario.ProjectSimulationId,
                ScenarioName = scenario.ScenarioName,
                Description = scenario.Description,
                SimulatedTaskId = scenario.SimulatedTaskId,
                SimulatedTaskTitle = task.Title,
                DelayDays = scenario.DelayDays,
                OriginalTaskEndDate = scenario.OriginalTaskEndDate,
                OriginalProjectEndDate = scenario.OriginalProjectEndDate,
                NewProjectEndDate = scenario.NewProjectEndDate,
                ProjectDelayDays = scenario.ProjectDelayDays,
                TotalAffectedTasks = scenario.TotalAffectedTasks,
                RiskLevel = scenario.RiskLevel,
                SimulatedAt = scenario.SimulatedAt,
                ImpactAnalysis = impact
            };
        }
    }
}
