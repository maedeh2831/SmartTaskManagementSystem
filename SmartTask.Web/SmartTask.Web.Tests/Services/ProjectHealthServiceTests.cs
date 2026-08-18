using Moq;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Risk;
using SmartTask.Web.Services.AI;
using SmartTask.Web.Services.Implementations;
using SmartTask.Web.Services.Interfaces;
using SmartTask.Web.Tests.TestHelpers;
using Xunit;

namespace SmartTask.Web.Tests.Services;

public class ProjectHealthServiceTests
{
    private readonly Mock<IDelayRiskService> _riskMock;

    public ProjectHealthServiceTests()
    {
        _riskMock = new Mock<IDelayRiskService>();
    }

    private ProjectHealthService CreateService(ApplicationDbContext context)
        => new(context, _riskMock.Object);

    [Fact]
    public async Task GetHealthAsync_MissingProject_ReturnsNull()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var result = await service.GetHealthAsync(99999, 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetHealthAsync_NoRisk_NoTasks_Health100()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        // The seed adds one sample task — soft-delete it to represent a task-less project.
        context.TaskItems.First().ViewState = false;
        await context.SaveChangesAsync();

        _riskMock.Setup(x => x.GetRiskOverviewAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new DelayRiskViewModel
            {
                ProjectId = 1,
                ProjectName = "TP",
                TotalOpenTasksCount = 0,
                TotalMembersCount = 0,
                RiskyDependencyChainsCount = 0
            });

        var service = CreateService(context);
        var result = await service.GetHealthAsync(seed.ProjectId, 1);

        Assert.NotNull(result);
        Assert.Equal(100, result!.HealthScore);
        Assert.Equal("excellent", result.HealthLevel);
    }

    [Fact]
    public async Task GetHealthAsync_OverdueTasks_ReduceScheduleHealth()
    {
        var seed = TestDbContextFactory.CreateSeeded(b => b.WithTask(t =>
        {
            t.Status = TaskStatusType.ToDo;
            t.DueDate = DateTime.Now.Date.AddDays(-3);
        }, out _));
        var context = seed.Context;

        _riskMock.Setup(x => x.GetRiskOverviewAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new DelayRiskViewModel
            {
                ProjectId = seed.ProjectId,
                ProjectName = "TP",
                OverdueTasksCount = 1,
                TotalOpenTasksCount = 2,
                TotalMembersCount = 1,
                OverloadedMembersCount = 0,
                RiskyDependencyChainsCount = 0
            });

        var service = CreateService(context);
        var result = await service.GetHealthAsync(seed.ProjectId, 1);

        // schedule = 100 - 50% = 50; delivery = 0% (no Done); health = 50*.30 + 100*.25 + 100*.20 + 0*.25 = 60
        Assert.Equal(50, result!.ScheduleHealth);
        Assert.Equal(60, result.HealthScore);
        Assert.Equal("fair", result.HealthLevel);
    }

    [Fact]
    public async Task GetHealthAsync_CompletedTasks_RaisesDeliveryHealth()
    {
        var seed = TestDbContextFactory.CreateSeeded(b =>
        {
            b.WithTask(t =>
            {
                t.Status = TaskStatusType.Done;
                t.CompletedDate = DateTime.Now;
            }, out _);
            b.WithTask(t =>
            {
                t.Status = TaskStatusType.ToDo;
            }, out _);
        });
        var context = seed.Context;

        _riskMock.Setup(x => x.GetRiskOverviewAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new DelayRiskViewModel
            {
                ProjectId = seed.ProjectId,
                ProjectName = "TP",
                TotalOpenTasksCount = 0,
                TotalMembersCount = 0,
                RiskyDependencyChainsCount = 0
            });

        var service = CreateService(context);
        var result = await service.GetHealthAsync(seed.ProjectId, 1);

        Assert.Equal(1, result!.CompletedTasksCount);
        Assert.Equal(3, result.TotalTasksCount); // base task + 2 added
        Assert.Equal(33, result.DeliveryHealth);
    }

    [Fact]
    public async Task GetHealthAsync_OverloadedMembers_ReduceWorkloadHealth()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        _riskMock.Setup(x => x.GetRiskOverviewAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new DelayRiskViewModel
            {
                ProjectId = seed.ProjectId,
                ProjectName = "TP",
                TotalOpenTasksCount = 0,
                TotalMembersCount = 4,
                OverloadedMembersCount = 2,
                RiskyDependencyChainsCount = 0
            });

        var service = CreateService(context);
        var result = await service.GetHealthAsync(seed.ProjectId, 1);

        // workload = 100 - 50% = 50
        Assert.Equal(50, result!.WorkloadHealth);
    }

    [Fact]
    public async Task GetHealthAsync_RiskyChains_ReduceDependencyHealth()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        _riskMock.Setup(x => x.GetRiskOverviewAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new DelayRiskViewModel
            {
                ProjectId = seed.ProjectId,
                ProjectName = "TP",
                TotalOpenTasksCount = 0,
                TotalMembersCount = 0,
                RiskyDependencyChainsCount = 4
            });

        var service = CreateService(context);
        var result = await service.GetHealthAsync(seed.ProjectId, 1);

        Assert.Equal(60, result!.DependencyHealth); // 100 - min(4*10, 100)
    }
}

public class DelayRiskServiceTests
{
    private readonly Mock<IWorkloadAnalysisService> _workloadMock;
    private readonly Mock<ITaskDependencyService> _dependencyMock;
    private readonly Mock<IAiClientService> _aiMock;

    public DelayRiskServiceTests()
    {
        _workloadMock = new Mock<IWorkloadAnalysisService>();
        _dependencyMock = new Mock<ITaskDependencyService>();
        _aiMock = new Mock<IAiClientService>();
    }

    private DelayRiskService CreateService(ApplicationDbContext context)
        => new(context, _workloadMock.Object, _dependencyMock.Object, _aiMock.Object);

    [Fact]
    public async Task GetRiskOverviewAsync_MissingProject_ReturnsNull()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var result = await service.GetRiskOverviewAsync(99999, 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetRiskOverviewAsync_CleanProject_LowRisk()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        _workloadMock.Setup(x => x.GetWorkloadAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new Models.ViewModels.Workload.WorkloadIndexViewModel
            {
                ProjectId = seed.ProjectId,
                ProjectName = "TP",
                ProjectWorkload = new()
            });
        _dependencyMock.Setup(x => x.GetProjectRiskOverviewAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Models.ViewModels.Dependency.DependencyRiskItemViewModel>());

        var service = CreateService(context);
        var result = await service.GetRiskOverviewAsync(seed.ProjectId, 1);

        Assert.NotNull(result);
        Assert.Equal(0, result!.RiskScore);
        Assert.Equal("low", result.RiskLevel);
        Assert.Equal("کم", result.RiskLevelDisplay);
    }

    [Fact]
    public async Task GetRiskOverviewAsync_OverdueTasks_ScoreScales()
    {
        var seed = TestDbContextFactory.CreateSeeded(b => b.WithTask(t =>
        {
            t.DueDate = DateTime.Now.Date.AddDays(-2);
        }, out _));
        var context = seed.Context;

        _workloadMock.Setup(x => x.GetWorkloadAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new Models.ViewModels.Workload.WorkloadIndexViewModel
            {
                ProjectId = seed.ProjectId,
                ProjectName = "TP",
                ProjectWorkload = new()
            });
        _dependencyMock.Setup(x => x.GetProjectRiskOverviewAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Models.ViewModels.Dependency.DependencyRiskItemViewModel>());

        var service = CreateService(context);
        var result = await service.GetRiskOverviewAsync(seed.ProjectId, 1);

        // 1 overdue / 2 open (base + added) -> overdueScore = 50% * 40 = 20
        Assert.Equal(1, result!.OverdueTasksCount);
        Assert.Equal(2, result.TotalOpenTasksCount);
        Assert.Equal(20, result.OverdueScore);
        Assert.Equal("low", result.RiskLevel);
    }

    [Fact]
    public async Task GetRiskOverviewAsync_AllOverdue_HighRisk()
    {
        var seed = TestDbContextFactory.CreateSeeded(b => b.WithTask(t =>
        {
            t.DueDate = DateTime.Now.Date.AddDays(-1);
        }, out _));
        var context = seed.Context;
        // base task also overdue
        var baseTask = context.TaskItems.First(t => t.Id == seed.TaskId);
        baseTask.DueDate = DateTime.Now.Date.AddDays(-5);

        _workloadMock.Setup(x => x.GetWorkloadAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new Models.ViewModels.Workload.WorkloadIndexViewModel
            {
                ProjectId = seed.ProjectId,
                ProjectName = "TP",
                ProjectWorkload = new()
            });
        _dependencyMock.Setup(x => x.GetProjectRiskOverviewAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Models.ViewModels.Dependency.DependencyRiskItemViewModel>());

        var service = CreateService(context);
        var result = await service.GetRiskOverviewAsync(seed.ProjectId, 1);

        Assert.Equal(2, result!.OverdueTasksCount);
        Assert.Equal(40, result.OverdueScore);
    }

    [Fact]
    public async Task GenerateNarrativeAsync_ReturnsAiCompletion()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        _workloadMock.Setup(x => x.GetWorkloadAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new Models.ViewModels.Workload.WorkloadIndexViewModel
            {
                ProjectId = seed.ProjectId,
                ProjectName = "TP",
                ProjectWorkload = new()
            });
        _dependencyMock.Setup(x => x.GetProjectRiskOverviewAsync(It.IsAny<int>()))
            .ReturnsAsync(new List<Models.ViewModels.Dependency.DependencyRiskItemViewModel>());
        _aiMock.Setup(x => x.GetCompletionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<double>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("تحلیل تولیدشده توسط AI");

        var service = CreateService(context);
        var result = await service.GenerateNarrativeAsync(seed.ProjectId, 1);

        Assert.Equal("تحلیل تولیدشده توسط AI", result);
    }
}
