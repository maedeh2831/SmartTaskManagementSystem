using Moq;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Implementations;
using SmartTask.Web.Tests.TestHelpers;
using Xunit;

namespace SmartTask.Web.Tests.Services;

public class ProjectServiceTests
{
    private readonly Mock<IGenericRepository<Project>> _repoMock;
    private readonly Mock<IUnitOfWork> _uowMock;

    public ProjectServiceTests()
    {
        _repoMock = new Mock<IGenericRepository<Project>>();
        _uowMock = new Mock<IUnitOfWork>();
    }

    private ProjectService CreateService(ApplicationDbContext context)
    {
        _repoMock.Setup(r => r.Query())
            .Returns(() => context.Projects.AsQueryable());

        return new ProjectService(_repoMock.Object, _uowMock.Object, context);
    }

    [Fact]
    public async Task GetDetailsAsync_ExistingProject_ReturnsWithMembers()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var result = await service.GetDetailsAsync(seed.ProjectId);

        Assert.NotNull(result);
        Assert.Equal(seed.ProjectId, result!.Id);
    }

    [Fact]
    public async Task ExistsByKeyAsync_True_WhenDuplicate()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);
        var projectKey = context.Projects.First().Key;

        var exists = await service.ExistsByKeyAsync(seed.WorkspaceId, projectKey);

        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsByKeyAsync_False_WhenMissing()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var exists = await service.ExistsByKeyAsync(seed.WorkspaceId, "NONEXISTENT");

        Assert.False(exists);
    }

    [Fact]
    public async Task ExistsByKeyAsync_ExcludeId_IgnoresCurrent()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);
        var project = context.Projects.First();

        var exists = await service.ExistsByKeyAsync(seed.WorkspaceId, project.Key, excludeId: project.Id);

        Assert.False(exists);
    }

    [Fact]
    public async Task CanManageProjectsAsync_WorkspaceOwner_True()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var result = await service.CanManageProjectsAsync(seed.WorkspaceId, seed.OwnerUserId);

        Assert.True(result);
    }

    [Fact]
    public async Task CanManageProjectAsync_WorkspaceOwner_True()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var result = await service.CanManageProjectAsync(seed.ProjectId, seed.OwnerUserId);

        Assert.True(result);
    }

    [Fact]
    public async Task CanManageProjectAsync_NonExistentProject_False()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var result = await service.CanManageProjectAsync(99999, seed.OwnerUserId);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_SetsViewStateFalse()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        await service.DeleteAsync(seed.ProjectId);

        var project = await context.Projects.FindAsync(seed.ProjectId);
        Assert.False(project!.ViewState);
    }

    [Fact]
    public async Task ArchiveAsync_MarksAsArchived()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        await service.ArchiveAsync(seed.ProjectId);

        var project = await context.Projects.FindAsync(seed.ProjectId);
        Assert.True(project!.IsArchived);
    }

    [Fact]
    public async Task ArchiveAsync_AlreadyArchived_NoThrow()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var project = context.Projects.First();
        project.IsArchived = true;
        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Should not throw
        await service.ArchiveAsync(seed.ProjectId);
        Assert.True(project.IsArchived);
    }

    [Fact]
    public async Task RestoreAsync_RemovesArchive()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var project = context.Projects.First();
        project.IsArchived = true;
        await context.SaveChangesAsync();

        var service = CreateService(context);
        await service.RestoreAsync(seed.ProjectId);

        var updated = await context.Projects.FindAsync(seed.ProjectId);
        Assert.False(updated!.IsArchived);
    }

    [Fact]
    public async Task UpdatePreferencesAsync_UpdatesColorAndIcon()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        await service.UpdatePreferencesAsync(seed.ProjectId, "#FF0000", "icon-star");

        var project = await context.Projects.FindAsync(seed.ProjectId);
        Assert.Equal("#FF0000", project!.Color);
        Assert.Equal("icon-star", project.Icon);
    }
}
