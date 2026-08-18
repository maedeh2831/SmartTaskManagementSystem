using Moq;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Implementations;
using SmartTask.Web.Services.Interfaces;
using SmartTask.Web.Tests.TestHelpers;
using Xunit;

namespace SmartTask.Web.Tests.Services;

public class SubTaskServiceTests
{
    private readonly Mock<IGenericRepository<SubTaskItem>> _repoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ITaskService> _taskMock;

    public SubTaskServiceTests()
    {
        _repoMock = new Mock<IGenericRepository<SubTaskItem>>();
        _uowMock = new Mock<IUnitOfWork>();
        _taskMock = new Mock<ITaskService>();
    }

    private SubTaskService CreateService(ApplicationDbContext context)
    {
        _repoMock.Setup(r => r.Query())
            .Returns(() => context.SubTaskItems.AsQueryable());
        return new SubTaskService(_repoMock.Object, _uowMock.Object, context, _taskMock.Object);
    }

    private static int _subTaskId = 1000;

    private static SubTaskItem SeedSubTask(ApplicationDbContext context, int taskItemId, bool completed = false)
    {
        var sub = new SubTaskItem
        {
            Id = _subTaskId++,
            TaskItemId = taskItemId,
            Title = "Subtask",
            IsCompleted = completed,
            ViewState = true
        };
        context.SubTaskItems.Add(sub);
        return sub;
    }

    [Fact]
    public async Task GetByTaskAsync_ReturnsOnlyMatching_OrderedByCreatedDate()
    {
        int otherId = 0;
        var seed = TestDbContextFactory.CreateSeeded(b => b.WithTask(t => t.Title = "Other", out otherId));
        var context = seed.Context;
        SeedSubTask(context, seed.TaskId);
        SeedSubTask(context, seed.TaskId);
        SeedSubTask(context, otherId);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetByTaskAsync(seed.TaskId);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByTaskAsync_FiltersSoftDeleted()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var sub = SeedSubTask(context, seed.TaskId);
        sub.ViewState = false;
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetByTaskAsync(seed.TaskId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task CanManageSubTaskAsync_DelegatesToTaskService()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var sub = SeedSubTask(context, seed.TaskId);
        await context.SaveChangesAsync();

        _taskMock.Setup(x => x.CanManageTaskAsync(seed.TaskId, 7)).ReturnsAsync(true);
        var service = CreateService(context);

        var result = await service.CanManageSubTaskAsync(sub.Id, 7);

        Assert.True(result);
        _taskMock.Verify(x => x.CanManageTaskAsync(seed.TaskId, 7), Times.Once);
    }

    [Fact]
    public async Task CanManageSubTaskAsync_MissingSubTask_ReturnsFalse()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var result = await service.CanManageSubTaskAsync(99999, 7);

        Assert.False(result);
    }

    [Fact]
    public async Task ToggleCompleteAsync_FlipsFlag()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var sub = SeedSubTask(context, seed.TaskId);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        await service.ToggleCompleteAsync(sub.Id);

        var updated = await context.SubTaskItems.FindAsync(sub.Id);
        Assert.True(updated!.IsCompleted);

        await service.ToggleCompleteAsync(sub.Id);
        updated = await context.SubTaskItems.FindAsync(sub.Id);
        Assert.False(updated!.IsCompleted);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletes()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var sub = SeedSubTask(context, seed.TaskId);
        await context.SaveChangesAsync();

        var service = CreateService(context);
        await service.DeleteAsync(sub.Id);

        var found = await context.SubTaskItems.FindAsync(sub.Id);
        Assert.NotNull(found);
        Assert.False(found!.ViewState);
    }
}
