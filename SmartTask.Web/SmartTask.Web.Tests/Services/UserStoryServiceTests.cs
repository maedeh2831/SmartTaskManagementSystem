using Moq;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Implementations;
using SmartTask.Web.Tests.TestHelpers;
using Xunit;

namespace SmartTask.Web.Tests.Services;

public class UserStoryServiceTests
{
    private readonly Mock<IGenericRepository<UserStory>> _repoMock;
    private readonly Mock<IUnitOfWork> _uowMock;

    public UserStoryServiceTests()
    {
        _repoMock = new Mock<IGenericRepository<UserStory>>();
        _uowMock = new Mock<IUnitOfWork>();
    }

    private UserStoryService CreateService(ApplicationDbContext context)
    {
        _repoMock.Setup(r => r.Query())
            .Returns(() => context.UserStories.AsQueryable());

        return new UserStoryService(_repoMock.Object, _uowMock.Object, context);
    }

    [Fact]
    public async Task GetDetailsAsync_ExistingStory_ReturnsWithRelations()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var result = await service.GetDetailsAsync(seed.UserStoryId);

        Assert.NotNull(result);
        Assert.Equal(seed.UserStoryId, result!.Id);
        Assert.NotNull(result.Project);
    }

    [Fact]
    public async Task GetBacklogStoriesAsync_ReturnsUnsprintedOnly()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var result = await service.GetBacklogStoriesAsync(seed.ProjectId);

        Assert.NotEmpty(result);
        Assert.All(result, s => Assert.Null(s.SprintId));
        Assert.All(result, s => Assert.True(s.ViewState));
    }

    [Fact]
    public async Task GetSprintStoriesAsync_ReturnsSprintedOnly()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var story = context.UserStories.First();
        story.SprintId = seed.SprintId;
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetSprintStoriesAsync(seed.SprintId);

        Assert.NotEmpty(result);
        Assert.All(result, s => Assert.Equal(seed.SprintId, s.SprintId));
    }

    [Fact]
    public async Task ExistsByTitleAsync_True_WhenDuplicate()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);
        var title = context.UserStories.First().Title;

        var exists = await service.ExistsByTitleAsync(seed.UserStoryId, title);

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsByTitleAsync_ExcludeId_IgnoresCurrent()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);
        var story = context.UserStories.First();

        var exists = await service.ExistsByTitleAsync(seed.ProjectId, story.Title, excludeId: story.Id);

        Assert.False(exists);
    }

    [Fact]
    public async Task CanManageBacklogAsync_WorkspaceOwner_True()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var result = await service.CanManageBacklogAsync(seed.ProjectId, seed.OwnerUserId);

        Assert.True(result);
    }

    [Fact]
    public async Task CanManageStoryAsync_InvalidStory_False()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var result = await service.CanManageStoryAsync(99999, seed.OwnerUserId);

        Assert.False(result);
    }

    [Fact]
    public async Task MoveToSprintAsync_SetsSprintId()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);
        var story = context.UserStories.First();

        await service.MoveToSprintAsync(story.Id, seed.SprintId);

        var updated = await context.UserStories.FindAsync(story.Id);
        Assert.Equal(seed.SprintId, updated!.SprintId);
    }

    [Fact]
    public async Task RemoveFromSprintAsync_ClearsSprintId()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var story = context.UserStories.First();
        story.SprintId = seed.SprintId;
        await context.SaveChangesAsync();

        var service = CreateService(context);
        await service.RemoveFromSprintAsync(story.Id);

        var updated = await context.UserStories.FindAsync(story.Id);
        Assert.Null(updated!.SprintId);
    }

    [Fact]
    public async Task ChangePriorityAsync_UpdatesPriority()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);
        var story = context.UserStories.First();

        await service.ChangePriorityAsync(story.Id, StoryPriorityType.High);

        var updated = await context.UserStories.FindAsync(story.Id);
        Assert.Equal(StoryPriorityType.High, updated!.Priority);
    }

    [Fact]
    public async Task ChangeStatusAsync_UpdatesStatus()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);
        var story = context.UserStories.First();

        await service.ChangeStatusAsync(story.Id, StoryStatusType.InProgress);

        var updated = await context.UserStories.FindAsync(story.Id);
        Assert.Equal(StoryStatusType.InProgress, updated!.Status);
    }

    [Fact]
    public async Task ReorderAsync_UpdatesOrder()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var stories = context.UserStories.Take(2).ToList();
        var orderedIds = new List<int> { stories[1].Id, stories[0].Id };

        var service = CreateService(context);
        await service.ReorderAsync(orderedIds);

        var first = await context.UserStories.FindAsync(stories[1].Id);
        var second = await context.UserStories.FindAsync(stories[0].Id);

        Assert.Equal(0, first!.Order);
        Assert.Equal(1, second!.Order);
    }

    [Fact]
    public async Task DeleteAsync_SetsViewStateFalse()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);
        var story = context.UserStories.First();

        await service.DeleteAsync(story.Id);

        var updated = await context.UserStories.FindAsync(story.Id);
        Assert.False(updated!.ViewState);
    }

    [Fact]
    public async Task ChangeOwnerAsync_UpdatesOwner()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);
        var story = context.UserStories.First();
        var newOwnerId = seed.OwnerUserId;

        await service.ChangeOwnerAsync(story.Id, newOwnerId);

        var updated = await context.UserStories.FindAsync(story.Id);
        Assert.Equal(newOwnerId, updated!.OwnerId);
    }

    [Fact]
    public async Task GetContributorsMapAsync_ReturnsCorrectMapping()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var result = await service.GetContributorsMapAsync(seed.ProjectId);

        Assert.NotNull(result);
        Assert.IsType<Dictionary<int, List<string>>>(result);
    }
}
