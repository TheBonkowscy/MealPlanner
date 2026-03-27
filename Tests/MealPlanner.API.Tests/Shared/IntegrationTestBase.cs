using MealPlanner.Persistence;
using MealPlanner.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace MealPlanner.API.Tests.Shared;

public abstract class IntegrationTestBase : IClassFixture<MealPlannerWebApplicationFactory>, IDisposable, IAsyncDisposable
{
    private readonly MealPlannerWebApplicationFactory _factory;
    private readonly WebApplicationFactoryClientOptions _options;
    
    protected IServiceScope ServiceScope;

    protected IntegrationTestBase()
    {
        _factory = new();
        _options = new()
        {
            BaseAddress = new Uri("https://localhost:5001"),
            AllowAutoRedirect = true
        };
        ServiceScope = _factory.Services.CreateScope();
        
        DatabaseContext.Database.EnsureDeleted();
        DatabaseContext.Database.EnsureCreated();
    }

    public HttpClient Client => _factory.CreateClient(_options);
    
    protected MealPlannerDbContext DatabaseContext => ServiceScope.ServiceProvider.GetRequiredService<MealPlannerDbContext>() ?? throw new Exception("Could not retrieve database instance");

    public void Dispose()
    {
        _factory.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _factory.DisposeAsync();
    }
}