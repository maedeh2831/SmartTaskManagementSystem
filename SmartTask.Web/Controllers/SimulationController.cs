using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.ViewModels.ProjectSimulation;
using SmartTask.Web.Services.Interfaces;

namespace SmartTask.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SimulationController : ControllerBase
    {
        private readonly IProjectSimulationEngine _simulationEngine;
        private readonly ICriticalPathAnalyzer _criticalPathAnalyzer;
        private readonly IProjectService _projectService;
        private readonly ApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<SimulationController> _logger;

        public SimulationController(
            IProjectSimulationEngine simulationEngine,
            ICriticalPathAnalyzer criticalPathAnalyzer,
            IProjectService projectService,
            ApplicationDbContext context,
            ICurrentUserService currentUserService,
            ILogger<SimulationController> logger)
        {
            _simulationEngine = simulationEngine;
            _criticalPathAnalyzer = criticalPathAnalyzer;
            _projectService = projectService;
            _context = context;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/simulation/project/{projectId}/critical-path
        /// Returns critical path analysis for a project
        /// </summary>
        [HttpGet("project/{projectId}/critical-path")]
        public async Task<ActionResult<CriticalPathDto>> GetCriticalPath(int projectId)
        {
            try
            {
                // Verify user has access to project
                if (!await _projectService.CanManageProjectAsync(projectId, _currentUserService.UserId))
                    return Forbid();

                var criticalPath = await _criticalPathAnalyzer.CalculateCriticalPathAsync(projectId);
                return Ok(criticalPath);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"Invalid project {projectId}");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating critical path for project {projectId}");
                return StatusCode(500, new { message = "Error calculating critical path" });
            }
        }

        /// <summary>
        /// POST /api/simulation/project/{projectId}/what-if
        /// Simulates delaying a task and returns impact analysis
        /// Body: { taskId, delayDays, scenarioName? }
        /// </summary>
        [HttpPost("project/{projectId}/what-if")]
        public async Task<ActionResult<ImpactAnalysisDto>> RunWhatIfScenario(
            int projectId,
            [FromBody] CreateSimulationScenarioRequest request)
        {
            try
            {
                // Verify user has access to project
                if (!await _projectService.CanManageProjectAsync(projectId, _currentUserService.UserId))
                    return Forbid();

                if (request.DelayDays < 1 || request.DelayDays > 365)
                    return BadRequest(new { message = "Delay days must be between 1 and 365" });

                var scenario = await _simulationEngine.RunScenarioAsync(
                    projectId,
                    request.TaskId,
                    request.DelayDays,
                    request.ScenarioName);

                _logger.LogInformation(
                    $"User {_currentUserService.UserId} ran what-if scenario: project {projectId}, " +
                    $"task {request.TaskId}, delay {request.DelayDays}d");

                return Ok(scenario);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"Invalid input for project {projectId}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error running what-if scenario for project {projectId}");
                return StatusCode(500, new { message = "Error running scenario" });
            }
        }

        /// <summary>
        /// GET /api/simulation/project/{projectId}/scenario/{scenarioId}
        /// Retrieves a specific simulation scenario
        /// </summary>
        [HttpGet("project/{projectId}/scenario/{scenarioId}")]
        public async Task<ActionResult<SimulationScenarioDto>> GetScenario(int projectId, int scenarioId)
        {
            try
            {
                // Verify user has access to project
                if (!await _projectService.CanManageProjectAsync(projectId, _currentUserService.UserId))
                    return Forbid();

                var scenario = await _simulationEngine.GetScenarioAsync(scenarioId);
                if (scenario == null)
                    return NotFound();

                return Ok(scenario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving scenario {scenarioId}");
                return StatusCode(500, new { message = "Error retrieving scenario" });
            }
        }

        /// <summary>
        /// GET /api/simulation/project/{projectId}/scenarios
        /// Lists all scenarios for a project simulation
        /// </summary>
        [HttpGet("project/{projectId}/scenarios")]
        public async Task<ActionResult<List<SimulationScenarioDto>>> GetProjectScenarios(int projectId)
        {
            try
            {
                // Verify user has access to project
                if (!await _projectService.CanManageProjectAsync(projectId, _currentUserService.UserId))
                    return Forbid();

                var simulationId = await _simulationEngine.GetOrCreateSimulationAsync(projectId);
                var scenarios = await _simulationEngine.GetProjectScenariosAsync(simulationId);

                return Ok(scenarios);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving scenarios for project {projectId}");
                return StatusCode(500, new { message = "Error retrieving scenarios" });
            }
        }

        /// <summary>
        /// POST /api/simulation/scenarios/{scenarioAId}/compare?scenarioBId={scenarioBId}
        /// Compares two scenarios and returns comparison metrics
        /// </summary>
        [HttpPost("scenarios/{scenarioAId}/compare")]
        public async Task<ActionResult<ScenarioComparisonDto>> CompareScenarios(
            int scenarioAId,
            [FromQuery] int scenarioBId)
        {
            try
            {
                // Verify both scenarios exist and user has access
                var scenarioA = await _context.SimulationScenarios.FirstOrDefaultAsync(s => s.Id == scenarioAId);
                var scenarioB = await _context.SimulationScenarios.FirstOrDefaultAsync(s => s.Id == scenarioBId);

                if (scenarioA == null || scenarioB == null)
                    return NotFound(new { message = "One or both scenarios not found" });

                var simA = await _context.ProjectSimulations.FirstOrDefaultAsync(s => s.Id == scenarioA.ProjectSimulationId);
                if (simA != null && !await _projectService.CanManageProjectAsync(simA.ProjectId, _currentUserService.UserId))
                    return Forbid();

                var comparison = await _simulationEngine.CompariousScenariosAsync(scenarioAId, scenarioBId);

                _logger.LogInformation(
                    $"User {_currentUserService.UserId} compared scenarios {scenarioAId} and {scenarioBId}");

                return Ok(comparison);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid scenarios for comparison");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing scenarios");
                return StatusCode(500, new { message = "Error comparing scenarios" });
            }
        }
    }
}
