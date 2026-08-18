using Microsoft.EntityFrameworkCore;
using SmartTask.Web.Data.Context;

namespace SmartTask.Web.Tests.TestHelpers;

/// <summary>
/// Creates isolated in-memory <see cref="ApplicationDbContext"/> instances
/// for unit tests. Each call gets a fresh, uniquely-named database so tests
/// never share state.
/// </summary>
public static class TestDbContextFactory
{
    private static int _counter;

    public static ApplicationDbContext Create()
    {
        var dbName = $"SmartTaskTestDb_{Guid.NewGuid():N}_{Interlocked.Increment(ref _counter)}";

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Builds the context and seeds the base data needed by most service tests:
    /// a workspace with its owner, the project, user stories, and tasks.
    /// Returns the builder so tests can read seeded ids (builder.Context).
    /// </summary>
    public static TestDataBuilder CreateSeeded(Action<TestDataBuilder>? configure = null)
    {
        var context = Create();
        var data = new TestDataBuilder(context);

        data.SeedBase();

        configure?.Invoke(data);

        context.SaveChanges();
        return data;
    }
}
