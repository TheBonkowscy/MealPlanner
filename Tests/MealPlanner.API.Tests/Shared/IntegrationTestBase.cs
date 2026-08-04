using MealPlanner.Persistence;
using MealPlanner.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MealPlanner.API.Tests.Shared;

public abstract class IntegrationTestBase : IDisposable, IAsyncDisposable
{
    private readonly MealPlannerWebApplicationFactory _factory;
    private readonly WebApplicationFactoryClientOptions _options;
    
    protected IServiceScope ServiceScope;

    protected IntegrationTestBase(MealPlannerWebApplicationFactory factory)
    {
        _factory = factory;
        _options = new()
        {
            BaseAddress = new Uri("https://localhost:5001"),
            AllowAutoRedirect = true
        };
        ServiceScope = _factory.Services.CreateScope();
        
        lock (_dbLock)
        {
            DatabaseContext.Database.EnsureDeleted();
            DatabaseContext.Database.EnsureCreated();
        }
    }

    private static readonly object _dbLock = new();

    public HttpClient Client => _factory.CreateClient(_options);
    
    protected MealPlannerDbContext DatabaseContext => ServiceScope.ServiceProvider.GetRequiredService<MealPlannerDbContext>() ?? throw new Exception("Could not retrieve database instance");

    public void Dispose()
    {
        ServiceScope.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await ValueTask.CompletedTask;
    }
}