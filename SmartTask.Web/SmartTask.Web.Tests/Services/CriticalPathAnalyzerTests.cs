using Microsoft.Extensions.Logging;
using Moq;
using SmartTask.Web.Data.Context;
using SmartTask.Web.Models.Entities;
using SmartTask.Web.Services.Implementations;
using SmartTask.Web.Tests.TestHelpers;
using Xunit;

namespace SmartTask.Web.Tests.Services;

/// <summary>
/// صفحه شبیه‌سازی فهرست وظایف را از TaskSlackTimes می‌سازد،
/// پس این فیلد باید همیشه تمام وظایف پروژه را برگرداند.
/// </summary>
public class CriticalPathAnalyzerTests
{
    private static CriticalPathAnalyzer CreateAnalyzer(ApplicationDbContext context)
        => new(context, new Mock<ILogger<CriticalPathAnalyzer>>().Object);

    [Fact]
    public async Task CalculateCriticalPathAsync_ReturnsEveryTask_ForTaskSelector()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;

        var result = await CreateAnalyzer(context).CalculateCriticalPathAsync(seed.ProjectId);

        // این همان منبعی است که کشویی «وظیفه مورد نظر» از آن پر می‌شود
        Assert.NotEmpty(result.TaskSlackTimes);
        Assert.All(result.TaskSlackTimes, t =>
        {
            Assert.True(t.TaskId > 0);
            Assert.False(string.IsNullOrWhiteSpace(t.TaskTitle));
        });
    }

    [Fact]
    public async Task CalculateCriticalPathAsync_ReturnsEmptyResult_WhenProjectHasNoTasks()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;

        // حذف نرم تمام وظایف پروژه
        foreach (var task in context.TaskItems.ToList())
            task.ViewState = false;
        await context.SaveChangesAsync();

        var result = await CreateAnalyzer(context).CalculateCriticalPathAsync(seed.ProjectId);

        Assert.Empty(result.TaskSlackTimes);
        Assert.Equal(0, result.TotalTasksInPath);
    }

    [Fact]
    public async Task CalculateCriticalPathAsync_Throws_ForUnknownProject()
    {
        var seed = TestDbContextFactory.CreateSeeded();

        await Assert.ThrowsAsync<ArgumentException>(
            () => CreateAnalyzer(seed.Context).CalculateCriticalPathAsync(999_999));
    }

    [Fact]
    public async Task CalculateCriticalPathAsync_DoesNotThrow_WhenDependencyPointsOutsideProject()
    {
        var seed = TestDbContextFactory.CreateSeeded();
        var context = seed.Context;

        // وابستگی به تسکی که در این پروژه نیست — قبلاً باعث استثنا می‌شد
        context.TaskDependencies.Add(new TaskDependency
        {
            TaskItemId = seed.TaskId,
            DependsOnTaskItemId = 987_654,
            IsRequired = true,
            ViewState = true
        });
        await context.SaveChangesAsync();

        var result = await CreateAnalyzer(context).CalculateCriticalPathAsync(seed.ProjectId);

        Assert.NotEmpty(result.TaskSlackTimes);
    }
}
