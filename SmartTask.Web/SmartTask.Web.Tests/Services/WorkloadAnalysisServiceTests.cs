using Moq;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Models.Enums;
using SmartTask.Web.Services.Implementations;
using SmartTask.Web.Services.Interfaces;
using SmartTask.Web.Tests.TestHelpers;
using Xunit;

namespace SmartTask.Web.Tests.Services;

public class WorkloadAnalysisServiceTests
{
    private readonly Mock<IProjectService> _projectMock;

    public WorkloadAnalysisServiceTests()
    {
        _projectMock = new Mock<IProjectService>();
    }

    private WorkloadAnalysisService CreateService(ApplicationDbContext context)
        => new(context, _projectMock.Object);

    [Fact]
    public async Task GetWorkloadAsync_MissingProject_ReturnsNull()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var result = await service.GetWorkloadAsync(99999, 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetWorkloadAsync_NoMembers_EmptyWorkload()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        _projectMock.Setup(x => x.CanManageProjectAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(false);

        var service = CreateService(context);
        var result = await service.GetWorkloadAsync(seed.ProjectId, 1);

        Assert.NotNull(result);
        Assert.Equal("Test Project", result!.ProjectName);
        Assert.Empty(result.ProjectWorkload);
        Assert.False(result.CanManage);
    }

    [Fact]
    public async Task GetWorkloadAsync_MemberWithSharedTask_UtilizationCalculated()
    {
        int id = 0;
        var seed = TestDbContextFactory.CreateSeeded(b =>
        {
            b.WithProjectMember(b.OwnerUserId, ProjectRoleType.Manager);
            b.WithTask(t =>
            {
                t.Estimate = 40;
                t.Status = TaskStatusType.InProgress;
                t.DueDate = null;
            }, out id);

            b.Context.TaskAssignments.Add(new TaskAssignment
            {
                TaskItemId = id,
                ApplicationUserId = b.OwnerUserId,
                ViewState = true
            });
        });
        var context = seed.Context;

        _projectMock.Setup(x => x.CanManageProjectAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        var service = CreateService(context);
        var result = await service.GetWorkloadAsync(seed.ProjectId, 1);

        var member = Assert.Single(result!.ProjectWorkload);
        Assert.Equal(40, member.CapacityHours);
        Assert.Equal(40, member.AssignedHours);
        Assert.Equal(100, member.UtilizationPercent);
        Assert.Equal("balanced", member.StatusLevel);
        Assert.Equal(1, member.TaskCount);
    }

    [Fact]
    public async Task GetWorkloadAsync_OverloadedMember_StatusOverloaded()
    {
        int id = 0;
        var seed = TestDbContextFactory.CreateSeeded(b =>
        {
            b.WithProjectMember(b.OwnerUserId, ProjectRoleType.Manager);
            b.WithTask(t =>
            {
                t.Estimate = 100;
                t.Status = TaskStatusType.InProgress;
            }, out id);

            b.Context.TaskAssignments.Add(new TaskAssignment
            {
                TaskItemId = id,
                ApplicationUserId = b.OwnerUserId,
                ViewState = true
            });
        });
        var context = seed.Context;

        _projectMock.Setup(x => x.CanManageProjectAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        var service = CreateService(context);
        var result = await service.GetWorkloadAsync(seed.ProjectId, 1);

        var member = result!.ProjectWorkload.Single();
        Assert.Equal(250, member.UtilizationPercent); // 100h / 40h
        Assert.Equal("overloaded", member.StatusLevel);
    }

    [Fact]
    public async Task GetWorkloadAsync_SharedTask_SplitsEstimateBetweenAssignees()
    {
        int id = 0;
        var seed = TestDbContextFactory.CreateSeeded(b =>
        {
            b.WithProjectMember(b.OwnerUserId, ProjectRoleType.Manager);
            b.WithProjectMember(b.MemberUserId, ProjectRoleType.Developer);
            b.WithTask(t =>
            {
                t.Estimate = 40;
                t.Status = TaskStatusType.InProgress;
            }, out id);

            b.Context.TaskAssignments.AddRange(
                new TaskAssignment { TaskItemId = id, ApplicationUserId = b.OwnerUserId, ViewState = true },
                new TaskAssignment { TaskItemId = id, ApplicationUserId = b.MemberUserId, ViewState = true });
        });
        var context = seed.Context;

        _projectMock.Setup(x => x.CanManageProjectAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        var service = CreateService(context);
        var result = await service.GetWorkloadAsync(seed.ProjectId, 1);

        Assert.Equal(2, result!.ProjectWorkload.Count);
        Assert.All(result.ProjectWorkload, m =>
        {
            Assert.Equal(20, m.AssignedHours); // 40 / 2
            Assert.Equal(50, m.UtilizationPercent);
            Assert.Equal("under", m.StatusLevel);
        });
    }

    [Fact]
    public async Task GetWorkloadAsync_UnassignedTasks_CountedAsUnassignedHours()
    {
        var seed = TestDbContextFactory.CreateSeeded(b =>
        {
            b.WithProjectMember(b.OwnerUserId, ProjectRoleType.Manager);
            b.WithTask(t =>
            {
                t.Estimate = 25;
                t.Status = TaskStatusType.InProgress;
            }, out _);
        });
        var context = seed.Context;

        _projectMock.Setup(x => x.CanManageProjectAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync(true);

        var service = CreateService(context);
        var result = await service.GetWorkloadAsync(seed.ProjectId, 1);

        Assert.Equal(25, result!.ProjectUnassignedHours);
    }

    [Fact]
    public async Task UpdateCapacityAsync_ClampsToMinimumOne()
    {
        var seed = TestDbContextFactory.CreateSeeded(b =>
        {
            b.WithProjectMember(b.OwnerUserId, ProjectRoleType.Manager);
        });
        var context = seed.Context;

        var memberId = context.ProjectMembers.First().Id;
        var service = CreateService(context);

        await service.UpdateCapacityAsync(memberId, 0);

        var member = await context.ProjectMembers.FindAsync(memberId);
        Assert.Equal(1, member!.WeeklyCapacityHours);
    }

    [Fact]
    public async Task UpdateCapacityAsync_SetsGivenCapacity()
    {
        var seed = TestDbContextFactory.CreateSeeded(b =>
        {
            b.WithProjectMember(b.OwnerUserId, ProjectRoleType.Manager);
        });
        var context = seed.Context;

        var memberId = context.ProjectMembers.First().Id;
        var service = CreateService(context);

        await service.UpdateCapacityAsync(memberId, 60);

        var member = await context.ProjectMembers.FindAsync(memberId);
        Assert.Equal(60, member!.WeeklyCapacityHours);
    }

    [Fact]
    public async Task UpdateCapacityAsync_MissingMember_NoOp()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        await service.UpdateCapacityAsync(99999, 60); // should not throw

        Assert.True(true);
    }

    [Fact]
    public async Task GetUserUtilizationAsync_NoMembership_ReturnsZero()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;
        var service = CreateService(context);

        var result = await service.GetUserUtilizationAsync(seed.ProjectId, 12345);

        Assert.Equal(0, result);
    }

    [Fact]
    public async Task GetUserUtilizationAsync_WithOpenTasks_ReturnsPercent()
    {
        int id = 0;
        var seed = TestDbContextFactory.CreateSeeded(b =>
        {
            b.WithProjectMember(b.OwnerUserId, ProjectRoleType.Manager);
            b.WithTask(t =>
            {
                t.Estimate = 20;
                t.Status = TaskStatusType.InProgress;
            }, out id);

            b.Context.TaskAssignments.Add(new TaskAssignment
            {
                TaskItemId = id,
                ApplicationUserId = b.OwnerUserId,
                ViewState = true
            });
        });
        var context = seed.Context;

        var service = CreateService(context);
        var result = await service.GetUserUtilizationAsync(seed.ProjectId, seed.OwnerUserId);

        Assert.Equal(50, result); // 20h / 40h capacity
    }

    [Fact]
    public async Task GetUserUtilizationAsync_IgnoresCompletedTasks()
    {
        int id = 0;
        var seed = TestDbContextFactory.CreateSeeded(b =>
        {
            b.WithProjectMember(b.OwnerUserId, ProjectRoleType.Manager);
            b.WithTask(t =>
            {
                t.Estimate = 80;
                t.Status = TaskStatusType.Done;
            }, out id);

            b.Context.TaskAssignments.Add(new TaskAssignment
            {
                TaskItemId = id,
                ApplicationUserId = b.OwnerUserId,
                ViewState = true
            });
        });
        var context = seed.Context;

        var service = CreateService(context);
        var result = await service.GetUserUtilizationAsync(seed.ProjectId, seed.OwnerUserId);

        Assert.Equal(0, result);
    }
}
