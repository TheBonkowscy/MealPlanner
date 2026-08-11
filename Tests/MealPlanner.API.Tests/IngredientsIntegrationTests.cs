using AwesomeAssertions;
using MealPlanner.API.Tests.Shared;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Shared.Ingredients;
using MealPlanner.Shared.Menus;
using Xunit;

namespace MealPlanner.API.Tests;

[Collection("IntegrationTests")]
public class IngredientsIntegrationTests(MealPlannerWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Get_ReturnsAllIngredients()
    {
        // Arrange
        await AddIngredientToDatabase(Ingredient.Create("Flour", [MeasureUnit.GlassCup, MeasureUnit.Gram, MeasureUnit.Tablespoon]));
        
        // Act
        var result = await Client.GetAsync(Constants.IngredientsRoute);

        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<GetIngredientsResponse>();
        response.Should().NotBeNull();
        response.Ingredients.Should().HaveCount(1);
    }

    private async Task AddIngredientToDatabase(Ingredient ingredient)
    {
        await DatabaseContext.Ingredients.AddAsync(ingredient);
        await DatabaseContext.SaveChangesAsync();
    }
}
