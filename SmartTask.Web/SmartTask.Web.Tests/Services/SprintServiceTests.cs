using Moq;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Infrastructure.Interfaces;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Implementations;
using SmartTask.Web.Tests.TestHelpers;
using Xunit;

namespace SmartTask.Web.Tests.Services;

public class SprintServiceTests
{
    private readonly Mock<IGenericRepository<Sprint>> _repoMock;
    private readonly Mock<IUnitOfWork> _uowMock;

    public SprintServiceTests()
    {
        _repoMock = new Mock<IGenericRepository<Sprint>>();
        _uowMock = new Mock<IUnitOfWork>();
    }

    private SprintService CreateService(ApplicationDbContext context)
    {
        _repoMock.Setup(r => r.Query())
            .Returns(() => context.Sprints.AsQueryable());
        return new SprintService(_repoMock.Object, _uowMock.Object, context);
    }

    private void SeedSprint(
        ApplicationDbContext context, int projectId, string name,
        SprintStatusType status = SprintStatusType.Planning,
        DateTime? start = null, DateTime? end = null)
    {
        context.Sprints.Add(new Sprint
        {
            Name = name,
            ProjectId = projectId,
            Status = status,
            StartDate = start ?? DateTime.Today,
            EndDate = end ?? DateTime.Today.AddDays(14),
            Capacity = 40
        });
    }

    [Fact]
    public async Task GetDetailsAsync_Existing_ReturnsSprint()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        SeedSprint(context, seed.ProjectId, "Sprint 1");
        await context.SaveChangesAsync();

        var sprintId = context.Sprints.First().Id;
        var service = CreateService(context);

        var result = await service.GetDetailsAsync(sprintId);

        Assert.NotNull(result);
        Assert.Equal("Sprint 1", result!.Name);
        Assert.NotNull(result.Project);
    }

    [Fact]
    public async Task GetByProjectAsync_ReturnsOnlyProjectSprints_NewestFirst()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        SeedSprint(context, seed.ProjectId, "Older", start: DateTime.Now.AddDays(-30), end: DateTime.Now.AddDays(-20));
        SeedSprint(context, seed.ProjectId, "Newer", start: DateTime.Now.AddDays(-5), end: DateTime.Now.AddDays(5));
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var result = await service.GetByProjectAsync(seed.ProjectId);

        Assert.Equal(2, result.Count);
        Assert.Equal("Newer", result[0].Name);
        Assert.Equal("Older", result[1].Name);
    }

    [Fact]
    public async Task ExistsByNameAsync_True_WhenDuplicate()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        SeedSprint(context, seed.ProjectId, "Sprint X");
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var exists = await service.ExistsByNameAsync(seed.ProjectId, "Sprint X");

        Assert.True(exists);
    }

    [Fact]
    public async Task HasDateOverlapAsync_DetectsOverlap()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        SeedSprint(context, seed.ProjectId, "Existing",
            start: DateTime.Now.AddDays(-1), end: DateTime.Now.AddDays(10));
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var overlaps = await service.HasDateOverlapAsync(
            seed.ProjectId, DateTime.Now, DateTime.Now.AddDays(5));

        Assert.True(overlaps);
    }

    [Fact]
    public async Task HasDateOverlapAsync_IgnoresCompletedAndCancelled()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        SeedSprint(context, seed.ProjectId, "Completed", SprintStatusType.Completed,
            start: DateTime.Now.AddDays(-1), end: DateTime.Now.AddDays(10));
        SeedSprint(context, seed.ProjectId, "Cancelled", SprintStatusType.Cancelled,
            start: DateTime.Now.AddDays(-1), end: DateTime.Now.AddDays(10));
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var overlaps = await service.HasDateOverlapAsync(
            seed.ProjectId, DateTime.Now, DateTime.Now.AddDays(5));

        Assert.False(overlaps);
    }

    [Fact]
    public async Task HasDateOverlapAsync_NoOverlap_ReturnsFalse()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        SeedSprint(context, seed.ProjectId, "Past",
            start: DateTime.Now.AddDays(-30), end: DateTime.Now.AddDays(-20));
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var overlaps = await service.HasDateOverlapAsync(
            seed.ProjectId, DateTime.Now, DateTime.Now.AddDays(5));

        Assert.False(overlaps);
    }

    [Fact]
    public async Task CanManageSprintsAsync_WorkspaceOwner_True()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var result = await service.CanManageSprintsAsync(seed.ProjectId, seed.OwnerUserId);

        Assert.True(result);
    }

    [Fact]
    public async Task CanManageSprintsAsync_WorkspaceAdmin_True()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var result = await service.CanManageSprintsAsync(seed.ProjectId, seed.OwnerUserId);

        Assert.True(result);
    }

    [Fact]
    public async Task CanManageSprintsAsync_RegularMember_False()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var result = await service.CanManageSprintsAsync(seed.ProjectId, seed.MemberUserId);

        Assert.False(result);
    }

    [Fact]
    public async Task ActivateAsync_OnlyOneActiveSprintPerProject()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        SeedSprint(context, seed.ProjectId, "First");
        SeedSprint(context, seed.ProjectId, "Second");
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var ids = context.Sprints.Select(x => x.Id).ToList();

        await service.ActivateAsync(ids[0]);

        Assert.Equal(SprintStatusType.Active, context.Sprints.Single(x => x.Id == ids[0]).Status);

        await service.ActivateAsync(ids[1]);

        // First is back to Planning, second is Active
        Assert.Equal(SprintStatusType.Planning, context.Sprints.Single(x => x.Id == ids[0]).Status);
        Assert.Equal(SprintStatusType.Active, context.Sprints.Single(x => x.Id == ids[1]).Status);
        Assert.Single(context.Sprints.Where(x => x.Status == SprintStatusType.Active));
    }
}
