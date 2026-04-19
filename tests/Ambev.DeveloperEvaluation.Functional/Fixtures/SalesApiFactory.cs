using Ambev.DeveloperEvaluation.ORM;
using Ambev.DeveloperEvaluation.WebApi;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional.Fixtures;

/// <summary>
/// WebApplicationFactory that:
///   (1) boots a disposable Postgres via Testcontainers,
///   (2) rewires DefaultContext to point to it and applies migrations,
///   (3) replaces JWT with <see cref="TestAuthHandler"/> so [Authorize]
///       passes without real tokens.
/// </summary>
public class SalesApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("postgres:13")
        .WithDatabase("ambev_func")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        using var scope = Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        await ctx.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _container.DisposeAsync();
        await base.DisposeAsync();
    }

    public async Task TruncateAsync()
    {
        using var scope = Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        await ctx.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE \"SaleItems\", \"Sales\" RESTART IDENTITY CASCADE;");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Swap DbContext to the test container.
            var dbOptions = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<DefaultContext>));
            if (dbOptions is not null) services.Remove(dbOptions);

            services.AddDbContext<DefaultContext>(o => o.UseNpgsql(
                _container.GetConnectionString(),
                b => b.MigrationsAssembly("Ambev.DeveloperEvaluation.ORM")));

            // Register TestAuth scheme and FORCE it as the default, overriding
            // whatever AddJwtAuthentication set up in Program.cs.
            services.AddAuthentication()
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });

            services.PostConfigure<AuthenticationOptions>(opts =>
            {
                opts.DefaultScheme = TestAuthHandler.SchemeName;
                opts.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                opts.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            });
        });
    }
}
