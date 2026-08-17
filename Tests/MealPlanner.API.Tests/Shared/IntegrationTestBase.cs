using MealPlanner.Domain;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Menus;
using MealPlanner.Persistence;
using MealPlanner.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MealPlanner.API.Tests.Shared;

public abstract class IntegrationTestBase : IDisposable, IAsyncDisposable
{
    private readonly MealPlannerWebApplicationFactory _factory;
    private readonly WebApplicationFactoryClientOptions _options;

    private readonly IServiceScope _serviceScope;

    protected IntegrationTestBase(MealPlannerWebApplicationFactory factory)
    {
        _factory = factory;
        _options = new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost:5001"),
            AllowAutoRedirect = true
        };
        _serviceScope = _factory.Services.CreateScope();
        
        lock (DbLock)
        {
            DatabaseContext.Database.EnsureDeleted();
            DatabaseContext.Database.EnsureCreated();

            ClearDatabase();
        }
    }

    private static readonly Lock DbLock = new();

    protected HttpClient Client => _factory.CreateClient(_options);
    
    protected MealPlannerDbContext DatabaseContext => _serviceScope.ServiceProvider.GetRequiredService<MealPlannerDbContext>() ?? throw new Exception("Could not retrieve database instance");
    
    private void ClearDatabase()
    {
        DatabaseContext.Meals.ExecuteDelete();
        DatabaseContext.Menus.ExecuteDelete();
        DatabaseContext.RecipeSteps.ExecuteDelete();
        DatabaseContext.Recipes.ExecuteDelete();
        DatabaseContext.Ingredients.ExecuteDelete();
    }
    

    protected async Task AddMenuToDatabase(Menu menu)
    {
        await DatabaseContext.Menus.AddAsync(menu);
        await DatabaseContext.SaveChangesAsync();
    }
    
    protected async Task AddRecipeToDatabase(Recipe recipe)
    {
        await DatabaseContext.Recipes.AddAsync(recipe);
        await DatabaseContext.SaveChangesAsync();
    }

    protected async Task AddIngredientToDatabase(Ingredient ingredient)
    {
        await DatabaseContext.Ingredients.AddAsync(ingredient);
        await DatabaseContext.SaveChangesAsync();
    }

    protected async Task AddUsedIngredientToDatabase(UsedIngredient ingredient)
    {
        await DatabaseContext.UsedIngredients.AddAsync(ingredient);
        await DatabaseContext.SaveChangesAsync();
    }
    
    public void Dispose()
    {
        _serviceScope.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await ValueTask.CompletedTask;
    }
}