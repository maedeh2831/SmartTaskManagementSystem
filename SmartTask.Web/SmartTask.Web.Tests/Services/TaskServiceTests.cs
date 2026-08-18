using Moq;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Implementations;
using SmartTask.Web.Services.Interfaces;
using SmartTask.Web.Tests.TestHelpers;
using Xunit;

namespace SmartTask.Web.Tests.Services;

public class TaskServiceTests
{
    private readonly Mock<IGenericRepository<TaskItem>> _repoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IUserStoryService> _storyMock;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly Mock<IActivityLogService> _activityMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;

    public TaskServiceTests()
    {
        _repoMock = new Mock<IGenericRepository<TaskItem>>();
        _uowMock = new Mock<IUnitOfWork>();
        _storyMock = new Mock<IUserStoryService>();
        _notificationMock = new Mock<INotificationService>();
        _activityMock = new Mock<IActivityLogService>();
        _currentUserMock = new Mock<ICurrentUserService>();
    }

    private TaskService CreateService(ApplicationDbContext context)
    {
        _repoMock.Setup(r => r.Query())
            .Returns(() => context.TaskItems.AsQueryable());
        _currentUserMock.Setup(c => c.UserId).Returns(1);

        return new TaskService(
            _repoMock.Object,
            _uowMock.Object,
            context,
            _storyMock.Object,
            _notificationMock.Object,
            _activityMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task GetDetailsAsync_ExistingTask_ReturnsWithProject()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var result = await service.GetDetailsAsync(seed.TaskId);

        Assert.NotNull(result);
        Assert.Equal("Sample task", result!.Title);
        Assert.NotNull(result.UserStory);
        Assert.NotNull(result.UserStory.Project);
    }

    [Fact]
    public async Task GetDetailsAsync_DeletedTask_ReturnsNull()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var task = context.TaskItems.First();
        task.ViewState = false;
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetDetailsAsync(seed.TaskId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUserStoryAsync_ReturnsOnlyMatching()
    {
        var seed = TestDbContextFactory.CreateSeeded(b =>
        {
            b.WithTask(t => t.Title = "Second", out _);
        });
        var context = seed.Context;
        var service = CreateService(context);

        var result = await service.GetByUserStoryAsync(seed.UserStoryId);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Title == "Sample task");
        Assert.Contains(result, t => t.Title == "Second");
    }

    [Fact]
    public async Task ExistsByTitleAsync_True_WhenDuplicate()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var exists = await service.ExistsByTitleAsync(seed.UserStoryId, "Sample task");

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsByTitleAsync_False_WhenTitleMissing()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var exists = await service.ExistsByTitleAsync(seed.UserStoryId, "No such title");

        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsByTitleAsync_ExcludeId_IgnoresCurrent()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var exists = await service.ExistsByTitleAsync(
            seed.UserStoryId, "Sample task", excludeId: seed.TaskId);

        Assert.False(exists);
    }

    [Fact]
    public async Task CanManageTaskAsync_MissingTask_ReturnsFalse()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var result = await service.CanManageTaskAsync(99999, 1);

        Assert.False(result);
    }

    [Fact]
    public async Task CanManageTaskAsync_DelegatesToStoryService()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        _storyMock.Setup(x => x.CanManageStoryAsync(seed.UserStoryId, 42))
            .ReturnsAsync(true);
        var service = CreateService(context);

        var result = await service.CanManageTaskAsync(seed.TaskId, 42);

        Assert.True(result);
        _storyMock.Verify(x => x.CanManageStoryAsync(seed.UserStoryId, 42), Times.Once);
    }

    [Fact]
    public async Task ChangeStatusAsync_SetsCompletedDate_WhenDone()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        _activityMock.Setup(x => x.LogAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int?>()))
            .Returns(Task.CompletedTask);
        _notificationMock.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>()))
            .Returns(Task.CompletedTask);
        var service = CreateService(context);

        await service.ChangeStatusAsync(seed.TaskId, TaskStatusType.Done);

        var task = await context.TaskItems.FindAsync(seed.TaskId);
        Assert.Equal(TaskStatusType.Done, task!.Status);
        Assert.NotNull(task.CompletedDate);
    }

    [Fact]
    public async Task ChangeStatusAsync_ClearsCompletedDate_WhenReopened()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var task = context.TaskItems.First();
        task.Status = TaskStatusType.Done;
        task.CompletedDate = DateTime.Now;
        await context.SaveChangesAsync();

        _activityMock.Setup(x => x.LogAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int?>()))
            .Returns(Task.CompletedTask);
        _notificationMock.Setup(x => x.CreateAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>()))
            .Returns(Task.CompletedTask);
        var service = CreateService(context);

        await service.ChangeStatusAsync(seed.TaskId, TaskStatusType.InProgress);

        var updated = await context.TaskItems.FindAsync(seed.TaskId);
        Assert.Equal(TaskStatusType.InProgress, updated!.Status);
        Assert.Null(updated.CompletedDate);
    }

    [Fact]
    public async Task AddAsync_LogsActivity()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var newTask = new TaskItem
        {
            UserStoryId = seed.UserStoryId,
            Title = "Brand new",
            ViewState = true
        };

        _repoMock.Setup(r => r.AddAsync(It.IsAny<TaskItem>()))
            .Callback<TaskItem>(t => context.TaskItems.Add(t))
            .Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);
        _activityMock.Setup(x => x.LogAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<int?>()))
            .Returns(Task.CompletedTask);

        await service.AddAsync(newTask);

        _activityMock.Verify(x => x.LogAsync(1, "ایجاد Task", It.IsAny<string?>(), It.IsAny<int?>()), Times.Once);
    }

    [Fact]
    public async Task GetProjectBoardAsync_FiltersCorrectly()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var result = await service.GetProjectBoardAsync(seed.ProjectId);

        Assert.NotEmpty(result);
        Assert.All(result, t => Assert.True(t.ViewState));
        Assert.All(result, t => Assert.True(t.UserStory.ViewState));
    }

    [Fact]
    public async Task GetProjectBoardAsync_FilterByPriority()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);
        var firstTask = context.TaskItems.First();
        firstTask.Priority = TaskPriorityType.High;
        await context.SaveChangesAsync();

        var result = await service.GetProjectBoardAsync(seed.ProjectId, priority: TaskPriorityType.High);

        Assert.NotEmpty(result);
        Assert.All(result, t => Assert.Equal(TaskPriorityType.High, t.Priority));
    }

    [Fact]
    public async Task DeleteAsync_SetsViewStateFalse()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        await service.DeleteAsync(seed.TaskId);

        var task = await context.TaskItems.FindAsync(seed.TaskId);
        Assert.False(task!.ViewState);
    }
}
