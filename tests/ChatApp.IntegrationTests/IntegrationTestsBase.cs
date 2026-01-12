using ChatApp.Infrastructure.Db;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.IntegrationTests;

public abstract class IntegrationTestsBase : IDisposable
{
    protected readonly AppDbContext Context;

    protected IntegrationTestsBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new AppDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
        Context.Dispose();
    }
}