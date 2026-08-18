using Moq;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Models.ViewModels.Dependency;
using SmartTask.Web.Services.Implementations;
using SmartTask.Web.Services.Interfaces;
using SmartTask.Web.Tests.TestHelpers;
using Xunit;
namespace SmartTask.Web.Tests.Services;

public class PriorityEngineServiceTests
{
    private readonly Mock<ITaskDependencyService> _dependencyMock;
    private readonly Mock<IWorkloadAnalysisService> _workloadMock;
    private readonly Mock<ITaskService> _taskMock;

    public PriorityEngineServiceTests()
    {
        _dependencyMock = new Mock<ITaskDependencyService>();
        _workloadMock = new Mock<IWorkloadAnalysisService>();
        _taskMock = new Mock<ITaskService>();
    }

    private PriorityEngineService CreateService(ApplicationDbContext context)
        => new(context, _dependencyMock.Object, _workloadMock.Object, _taskMock.Object);

    private void MockNoImpact()
        => _dependencyMock
            .Setup(x => x.GetImpactedTasksAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<ImpactedTaskViewModel>());

    [Fact]
    public async Task GetSuggestionAsync_NoDueDate_NoDeps_NoAssignees_Scores10AndLowest()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        MockNoImpact();
        _workloadMock.Setup(x => x.GetUserUtilizationAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(0);
        _taskMock.Setup(x => x.CanManageTaskAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        var service = CreateService(context);
        var result = await service.GetSuggestionAsync(seed.TaskId, 1);

        Assert.Equal(10, result.UrgencyScore);
        Assert.Equal(0, result.DependencyScore);
        Assert.Equal(0, result.WorkloadScore);
        Assert.Equal(10, result.TotalScore);
        Assert.Equal(TaskPriorityType.Lowest, result.SuggestedPriority);
        Assert.Contains(result.Reasons, r => r.Contains("موعد مشخصی"));
    }

    [Fact]
    public async Task GetSuggestionAsync_DueToday_Urgency40()
    {
        var seed = TestDbContextFactory.CreateSeeded(b => b.WithTask(t =>
        {
            t.DueDate = DateTime.Now.Date;
        }, out _));
        var context = seed.Context;
        MockNoImpact();
        _workloadMock.Setup(x => x.GetUserUtilizationAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(0);

        var service = CreateService(context);
        var taskId = context.TaskItems.OrderBy(x => x.Id).Last().Id;
        var result = await service.GetSuggestionAsync(taskId, 1);

        Assert.Equal(40, result.UrgencyScore);
        Assert.Contains(result.Reasons, r => r.Contains("امروز"));
    }

    [Fact]
    public async Task GetSuggestionAsync_OverdueByFiveDays_Urgency40()
    {
        var seed = TestDbContextFactory.CreateSeeded(b => b.WithTask(t =>
        {
            t.DueDate = DateTime.Now.Date.AddDays(-5);
        }, out _));
        var context = seed.Context;
        MockNoImpact();
        _workloadMock.Setup(x => x.GetUserUtilizationAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(0);

        var service = CreateService(context);
        var taskId = context.TaskItems.OrderBy(x => x.Id).Last().Id;
        var result = await service.GetSuggestionAsync(taskId, 1);

        Assert.Equal(40, result.UrgencyScore);
        Assert.Contains(result.Reasons, r => r.Contains("۵ روز") || r.Contains("5 روز") || r.Contains("گذشته"));
    }

    [Fact]
    public async Task GetSuggestionAsync_DueInFifteenDays_Urgency20()
    {
        var seed = TestDbContextFactory.CreateSeeded(b => b.WithTask(t =>
        {
            t.DueDate = DateTime.Now.Date.AddDays(15);
        }, out _));
        var context = seed.Context;
        MockNoImpact();
        _workloadMock.Setup(x => x.GetUserUtilizationAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(0);

        var service = CreateService(context);
        var taskId = context.TaskItems.OrderBy(x => x.Id).Last().Id;
        var result = await service.GetSuggestionAsync(taskId, 1);

        // 40 - 15 * (40/30) = 40 - 20 = 20
        Assert.Equal(20, result.UrgencyScore);
    }

    [Fact]
    public async Task GetSuggestionAsync_RequiredDependencyChain_AddsDependencyScore()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        _dependencyMock
            .Setup(x => x.GetImpactedTasksAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(new List<ImpactedTaskViewModel>
            {
                new() { TaskId = 900, Title = "Dep 1", IsRequiredChain = true, Depth = 1 },
                new() { TaskId = 901, Title = "Dep 2", IsRequiredChain = true, Depth = 2 },
                new() { TaskId = 902, Title = "Dep 3", IsRequiredChain = false, Depth = 1 }
            });
        _workloadMock.Setup(x => x.GetUserUtilizationAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(0);

        var service = CreateService(context);
        var result = await service.GetSuggestionAsync(seed.TaskId, 1);

        // 2 required * 7 = 14, optional ignored
        Assert.Equal(14, result.DependencyScore);
    }

    [Fact]
    public async Task GetSuggestionAsync_OverloadedAssignee_WorkloadScore25()
    {
        int id = 0;
        var seed = TestDbContextFactory.CreateSeeded(b => b.WithTask(t =>
        {
            t.DueDate = DateTime.Now.Date;
        }, out id));
        var context = seed.Context;
        MockNoImpact();

        var userId = context.Users.First().Id;
        context.TaskAssignments.Add(new()
        {
            TaskItemId = id,
            ApplicationUserId = userId
        });
        await context.SaveChangesAsync();

        _workloadMock.Setup(x => x.GetUserUtilizationAsync(It.IsAny<int>(), userId))
            .ReturnsAsync(120);

        var service = CreateService(context);
        var result = await service.GetSuggestionAsync(id, 1);

        Assert.Equal(25, result.WorkloadScore);
        Assert.Equal(40, result.UrgencyScore);
        Assert.Equal(65, result.TotalScore);
        Assert.Equal(TaskPriorityType.High, result.SuggestedPriority);
    }

    [Fact]
    public async Task GetSuggestionAsync_NearCapacity_WorkloadScore15()
    {
        int id = 0;
        var seed = TestDbContextFactory.CreateSeeded(b => b.WithTask(t =>
        {
            t.DueDate = DateTime.Now.Date;
        }, out id));
        var context = seed.Context;
        MockNoImpact();

        var userId = context.Users.First().Id;
        context.TaskAssignments.Add(new()
        {
            TaskItemId = id,
            ApplicationUserId = userId
        });
        await context.SaveChangesAsync();

        _workloadMock.Setup(x => x.GetUserUtilizationAsync(It.IsAny<int>(), userId))
            .ReturnsAsync(85);

        var service = CreateService(context);
        var result = await service.GetSuggestionAsync(id, 1);

        Assert.Equal(15, result.WorkloadScore);
    }

    [Fact]
    public async Task GetSuggestionAsync_MissingTask_Throws()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetSuggestionAsync(99999, 1));
    }

    [Fact]
    public async Task ApplySuggestionAsync_WithoutPermission_ThrowsUnauthorized()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        MockNoImpact();
        _workloadMock.Setup(x => x.GetUserUtilizationAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(0);
        _taskMock.Setup(x => x.CanManageTaskAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(false);

        var service = CreateService(context);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.ApplySuggestionAsync(seed.TaskId, 1));
    }

    [Fact]
    public async Task ApplySuggestionAsync_WithPermission_UpdatesTaskPriority()
    {
        int id = 0;
        var seed = TestDbContextFactory.CreateSeeded(b => b.WithTask(t =>
        {
            t.DueDate = DateTime.Now.Date.AddDays(30);
            t.Priority = TaskPriorityType.Medium;
        }, out id));
        var context = seed.Context;
        MockNoImpact();
        _workloadMock.Setup(x => x.GetUserUtilizationAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(0);
        _taskMock.Setup(x => x.CanManageTaskAsync(id, 1)).ReturnsAsync(true);

        var service = CreateService(context);
        await service.ApplySuggestionAsync(id, 1);

        var updated = await context.TaskItems.FindAsync(id);
        // Due in 30 days -> urgency 0, total 0 -> Lowest
        Assert.Equal(TaskPriorityType.Lowest, updated!.Priority);
    }
}
