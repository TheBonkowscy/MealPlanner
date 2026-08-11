using AwesomeAssertions;
using MealPlanner.API.Tests.Shared;
using MealPlanner.Domain;
using MealPlanner.Shared.Menus;
using MealPlanner.Shared.Recipes;
using Xunit;

namespace MealPlanner.API.Tests;

[Collection("IntegrationTests")]
public class MealsIntegrationTests : IntegrationTestBase
{
    public MealsIntegrationTests(MealPlannerWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Get_ReturnsAllMeals_WhenNoQueryProvided()
    {
        // Arrange
        await AddMealToDatabase(Recipe.Create("Burger"));

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
        await AddMealToDatabase(Recipe.Create(uniqueMealName));

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
        await AddMealToDatabase(Recipe.Create("Pizza"));

        // Act
        var result = await Client.GetAsync($"{Constants.RecipesRoute}?q=salad");

        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<GetRecipesResponse>();
        response.Should().NotBeNull();
        response!.Recipes.Should().BeEmpty();
    }

    private async Task AddMealToDatabase(Recipe recipe)
    {
        await DatabaseContext.Recipes.AddAsync(recipe);
        await DatabaseContext.SaveChangesAsync();
    }
}
