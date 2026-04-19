using Ambev.DeveloperEvaluation.ORM;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Fixtures;

/// <summary>
/// Spins up a disposable PostgreSQL container, applies the app's migrations
/// and hands out a fresh DbContext per call.
/// </summary>
public sealed class PostgresContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:13")
        .WithDatabase("ambev_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public async Task DisposeAsync() => await _container.DisposeAsync();

    public DefaultContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<DefaultContext>()
            .UseNpgsql(ConnectionString, b => b.MigrationsAssembly("Ambev.DeveloperEvaluation.ORM"))
            .Options;
        return new DefaultContext(options);
    }

    public async Task TruncateAsync()
    {
        await using var context = CreateContext();
        // Order matters: children first.
        await context.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE \"SaleItems\", \"Sales\" RESTART IDENTITY CASCADE;");
    }
}
