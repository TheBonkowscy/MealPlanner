using System.Net;
using AwesomeAssertions;
using MealPlanner.API.Tests.Shared;
using MealPlanner.Domain;
using MealPlanner.Shared.Menus;
using MealPlanner.Shared.Recipes;
using MealPlanner.Shared.Recipes.Responses;
using MealPlanner.Tests.Shared.Factories;
using Xunit;

namespace MealPlanner.API.Tests;

[Collection("IntegrationTests")]
public class RecipesIntegrationTests(MealPlannerWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Get_ReturnsAllMeals_WhenNoQueryProvided()
    {
        // Arrange
        await AddMealToDatabase(TestRecipes.Create("Burger"));

        // Act
        var result = await Client.GetAsync(Constants.RecipesRoute);

        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<GetRecipesResponse>();
        response.Should().NotBeNull();
        response.Recipes.Should().HaveCount(1);
    }

    [Fact]
    public async Task Get_ReturnsFilteredMeals_WhenQueryProvided()
    {
        // Arrange
        var uniqueMealName = "Unique Sandwich";
        await AddMealToDatabase(TestRecipes.Create(uniqueMealName));

        // Act
        var result = await Client.GetAsync($"{Constants.RecipesRoute}?q=sandw");

        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<GetRecipesResponse>();
        response.Should().NotBeNull();
        response.Recipes.Should().HaveCount(1);
        response.Recipes.Should().Contain(m => m.Name == uniqueMealName);
    }

    [Fact]
    public async Task Get_ReturnsEmptyList_WhenNoMatchesFound()
    {
        // Arrange
        await AddMealToDatabase(TestRecipes.Create("Pizza"));

        // Act
        var result = await Client.GetAsync($"{Constants.RecipesRoute}?q=salad");

        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<GetRecipesResponse>();
        response.Should().NotBeNull();
        response!.Recipes.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenNoMealById()
    {
        // Act
        var result = await Client.GetAsync($"{Constants.RecipesRoute}/{-200}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_ReturnsOk_WhenMealWasFound()
    {
        // Arrange 
        var recipe = TestRecipes.Create("Pizza");
        await AddMealToDatabase(recipe);
        
        // Act
        var result = await Client.GetAsync($"{Constants.RecipesRoute}/{recipe.Id}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task AddMealToDatabase(Recipe recipe)
    {
        await DatabaseContext.Recipes.AddAsync(recipe);
        await DatabaseContext.SaveChangesAsync();
    }
}
