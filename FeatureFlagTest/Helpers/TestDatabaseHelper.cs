using FeatureFlagCore.Data;
using Microsoft.EntityFrameworkCore;

namespace FeatureFlags.Tests.Helpers;

public static class TestDatabaseHelper
{
    public static FeatureFlagDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<FeatureFlagDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        var context = new FeatureFlagDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
