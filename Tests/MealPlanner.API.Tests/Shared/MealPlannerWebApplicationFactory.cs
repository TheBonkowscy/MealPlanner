using DotNet.Testcontainers.Containers;
using MealPlanner.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace MealPlanner.API.Tests.Shared;

public class MealPlannerWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string DatabaseName = "MealPlanner";
    private const string DatabaseUsername = "postgres";
    private const string DatabasePassword = "12345";
    private const short DatabasePort = 5432;
    
    private static readonly TaskFactory TaskFactory = new(CancellationToken.None,
        TaskCreationOptions.None,
        TaskContinuationOptions.None,
        TaskScheduler.Default);

    private static readonly PostgreSqlContainer PostgresqlContainer = new PostgreSqlBuilder("postgres:18.3")
        .WithDatabase(DatabaseName)
        .WithUsername(DatabaseUsername)
        .WithPassword(DatabasePassword)
        .WithPortBinding(DatabasePort, true)
        .Build();

    private static readonly List<DockerContainer> ContainersToRun = [PostgresqlContainer];

    static MealPlannerWebApplicationFactory()
    {
        ContainersToRun.ForEach(container =>
        {
            TaskFactory.StartNew(async() => await container.StartAsync())
                .Unwrap()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        });
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<MealPlannerDbContext>();
            services.AddDbContext<MealPlannerDbContext>(options =>
                options.UseNpgsql(PostgresqlContainer.GetConnectionString()));
        });
    }
    
}