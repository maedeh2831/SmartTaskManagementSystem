using Moq;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Implementations;
using SmartTask.Web.Services.Interfaces;
using SmartTask.Web.Tests.TestHelpers;
using Xunit;

namespace SmartTask.Web.Tests.Services;

public class TaskDependencyServiceTests
{
    private readonly Mock<ITaskService> _taskMock;

    public TaskDependencyServiceTests()
    {
        _taskMock = new Mock<ITaskService>();
    }

    private TaskDependencyService CreateService(ApplicationDbContext context)
        => new(context, _taskMock.Object);

    private static void SeedTask(ApplicationDbContext context, int id, string title, TaskStatusType status = TaskStatusType.ToDo, DateTime? due = null)
    {
        context.TaskItems.Add(new TaskItem
        {
            Id = id,
            UserStoryId = context.UserStories.First().Id,
            Title = title,
            Status = status,
            DueDate = due,
            ViewState = true
        });
    }

    [Fact]
    public async Task AddDependencyAsync_SelfDependency_Rejected()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var (success, error) = await service.AddDependencyAsync(seed.TaskId, seed.TaskId, true);

        Assert.False(success);
        Assert.Contains("خودش", error);
    }

    [Fact]
    public async Task AddDependencyAsync_Duplicate_Rejected()
    {
        int depId = 0;
        var seed = TestDbContextFactory.CreateSeeded(b => b.WithTask(t => t.Title = "Dep", out depId));
        var context = seed.Context;
        var service = CreateService(context);

        await service.AddDependencyAsync(seed.TaskId, depId, true);
        var (success, error) = await service.AddDependencyAsync(seed.TaskId, depId, true);

        Assert.False(success);
        Assert.Contains("قبلاً", error);
    }

    [Fact]
    public async Task AddDependencyAsync_CreatesCycle_Rejected()
    {
        var seed = TestDbContextFactory.CreateSeeded(b =>
        {
            b.WithTask(t => t.Title = "A", out var a);
            b.WithTask(t => t.Title = "B", out var bb);
            b.WithDependency(a, b.TaskId, true);   // A -> base
            b.WithDependency(b.TaskId, bb, true);  // base -> B
        });
        var context = seed.Context;

        var baseId = seed.TaskId;
        var bId = context.TaskItems.OrderBy(x => x.Id).Last().Id;
        var service = CreateService(context);

        // base -> B already exists (from seed). Try B -> base which would close the cycle.
        var (success, error) = await service.AddDependencyAsync(bId, baseId, true);

        Assert.False(success);
        Assert.Contains("چرخه", error);
    }

    [Fact]
    public async Task AddDependencyAsync_Valid_AddsAndSaves()
    {
        int depId = 0;
        var seed = TestDbContextFactory.CreateSeeded(b => b.WithTask(t => t.Title = "Dep", out depId));
        var context = seed.Context;
        var service = CreateService(context);

        var (success, error) = await service.AddDependencyAsync(seed.TaskId, depId, true);

        Assert.True(success);
        Assert.Null(error);
        Assert.Equal(1, context.TaskDependencies.Count());
    }

    [Fact]
    public async Task RemoveDependencyAsync_Existing_Removes()
    {
        int depId = 0;
        var seed = TestDbContextFactory.CreateSeeded(b => b.WithTask(t => t.Title = "Dep", out depId));
        var context = seed.Context;
        var service = CreateService(context);

        var (_, _) = await service.AddDependencyAsync(seed.TaskId, depId, true);
        var depIdToRemove = context.TaskDependencies.First().Id;

        var removed = await service.RemoveDependencyAsync(depIdToRemove);

        Assert.True(removed);
        Assert.Empty(context.TaskDependencies);
    }

    [Fact]
    public async Task RemoveDependencyAsync_Missing_ReturnsFalse()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var removed = await service.RemoveDependencyAsync(99999);

        Assert.False(removed);
    }

    [Fact]
    public async Task GetImpactedTasksAsync_Chain_ReturnsDependents()
    {
        var seed = TestDbContextFactory.CreateSeeded(b =>
        {
            b.WithTask(t => t.Title = "Dep1", out var d1);
            b.WithTask(t => t.Title = "Dep2", out var d2);
            b.WithDependency(d1, b.TaskId, true);
            b.WithDependency(d2, d1, false);
        });
        var context = seed.Context;

        var d1 = context.TaskItems.Single(t => t.Title == "Dep1").Id;
        var d2 = context.TaskItems.Single(t => t.Title == "Dep2").Id;

        var service = CreateService(context);
        var result = await service.GetImpactedTasksAsync(seed.TaskId, 3);

        Assert.Equal(2, result.Count);

        var first = result.Single(x => x.TaskId == d1);
        Assert.True(first.IsRequiredChain);
        Assert.Equal(1, first.Depth);

        var second = result.Single(x => x.TaskId == d2);
        Assert.False(second.IsRequiredChain); // optional edge breaks the required chain
        Assert.Equal(2, second.Depth);
    }

    [Fact]
    public async Task GetWidgetAsync_MissingTask_Throws()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetWidgetAsync(99999, 1));
    }

    [Fact]
    public async Task GetWidgetAsync_ListsDependsOnAndDependents()
    {
        var seed = TestDbContextFactory.CreateSeeded(b =>
        {
            b.WithTask(t => t.Title = "Upstream", out var up);
            b.WithTask(t => t.Title = "Downstream", out var down);
            b.WithDependency(b.TaskId, up, true);
            b.WithDependency(down, b.TaskId, false);
        });
        var context = seed.Context;

        var up = context.TaskItems.Single(t => t.Title == "Upstream").Id;
        var down = context.TaskItems.Single(t => t.Title == "Downstream").Id;
        _taskMock.Setup(x => x.CanManageTaskAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        var service = CreateService(context);
        var widget = await service.GetWidgetAsync(seed.TaskId, 1);

        Assert.True(widget.CanManage);
        Assert.Single(widget.DependsOn);
        Assert.Equal(up, widget.DependsOn[0].TaskId);
        Assert.True(widget.DependsOn[0].IsRequired);

        Assert.Single(widget.Dependents);
        Assert.Equal(down, widget.Dependents[0].TaskId);
        Assert.False(widget.Dependents[0].IsRequired);

        // Available = project tasks minus self and linked (upstream)
        Assert.DoesNotContain(widget.AvailableTasks, t => t.Value == seed.TaskId.ToString());
        Assert.DoesNotContain(widget.AvailableTasks, t => t.Value == up.ToString());
    }

    [Fact]
    public async Task GetProjectRiskOverviewAsync_OnlyReturnsRiskyDelayedTasks()
    {
        var seed = TestDbContextFactory.CreateSeeded(b =>
        {
            b.WithTask(t =>
            {
                t.Title = "Late with impact";
                t.DueDate = DateTime.Now.Date.AddDays(-2);
            }, out var late);
            b.WithTask(t =>
            {
                t.Title = "On time";
                t.DueDate = DateTime.Now.Date.AddDays(5);
            }, out var onTime);
            b.WithTask(t =>
            {
                t.Title = "Late no impact";
                t.DueDate = DateTime.Now.Date.AddDays(-1);
            }, out var lateNoImpact);

            b.WithDependency(b.TaskId, late, true);
        });
        var context = seed.Context;

        var service = CreateService(context);
        var result = await service.GetProjectRiskOverviewAsync(seed.ProjectId);

        var item = Assert.Single(result);
        Assert.Equal("Late with impact", item.Title);
        Assert.Equal(2, item.DelayDays);
        Assert.Equal(1, item.ImpactedTaskCount);
    }
}
