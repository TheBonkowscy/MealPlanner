using System.Net;
using AwesomeAssertions;
using MealPlanner.API.Tests.Shared;
using MealPlanner.Domain;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Shared.Menus;
using MealPlanner.Shared.Recipes;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;
using MealPlanner.Tests.Shared.Factories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MealPlanner.API.Tests;

[Collection("IntegrationTests")]
public class RecipesIntegrationTests(MealPlannerWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Get_ReturnsAllMeals_WhenNoQueryProvided()
    {
        // Arrange
        await AddRecipeToDatabase(TestRecipes.Create("Burger"));

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
        await AddRecipeToDatabase(TestRecipes.Create(uniqueMealName));

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
        await AddRecipeToDatabase(TestRecipes.Create("Pizza"));

        // Act
        var result = await Client.GetAsync($"{Constants.RecipesRoute}?q=salad");

        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<GetRecipesResponse>();
        response.Should().NotBeNull();
        response!.Recipes.Should().BeEmpty();
    }

    [Fact]
    public async Task Post_ReturnsOk_WhenRecipeIsCreated()
    {
        // Arrange
        var testIngredient = TestIngredients.Create();
        DatabaseContext.Ingredients.Add(testIngredient);
        await DatabaseContext.SaveChangesAsync();
        var request = CreateNewRecipeRequest($"New Recipe_{Guid.NewGuid()}", testIngredient);

        // Act
        var result = await Client.PostAsJsonAsync(Constants.RecipesRoute, request);
        
        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<CreateRecipeResponse>();
        response.Should().NotBeNull();
    }

    private static CreateRecipeRequest CreateNewRecipeRequest(string recipeName, Ingredient ingredient) =>
        new(recipeName,
            1,
            [new AddIngredientRequest(ingredient.Id, 1, ingredient.ApplicableUnits.First().ToString())],
            [new AddStepRequest(1, "Test")]);

    [Fact]
    public async Task Post_ReturnsBadRequest_WhenRecipeIsNotValid()
    {
        // Arrange
        var testIngredient = TestIngredients.Create();
        var request = CreateNewRecipeRequest($"New Recipe_{Guid.NewGuid()}", testIngredient);
        request.Ingredients.Clear();
        request.Steps.Clear();

        // Act
        var result = await Client.PostAsJsonAsync(Constants.RecipesRoute, request);
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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
        await AddRecipeToDatabase(recipe);
        
        // Act
        var result = await Client.GetAsync($"{Constants.RecipesRoute}/{recipe.Id}");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        // Arrange
        var recipe = TestRecipes.Create();
        await AddRecipeToDatabase(recipe);
        
        // Act
        var result = await Client.DeleteAsync($"{Constants.RecipesRoute}/{recipe.Id}");
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    private async Task AddRecipeToDatabase(Recipe recipe)
    {
        await DatabaseContext.Recipes.AddAsync(recipe);
        await DatabaseContext.SaveChangesAsync();
    }
}
