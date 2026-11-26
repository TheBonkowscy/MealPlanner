using AwesomeAssertions;
using MealPlanner.Shared.Recipes.Responses;
using MealPlanner.UI.Models;
using MealPlanner.UI.Services;

namespace MealPlanner.UI.Tests;

public class MealMapperTests
{
    private readonly MealMapper _sut = new();

    [Fact]
    public void MapPanelRecipes_ReturnsRecipesOrderedByName_WithTheirOwnServings()
    {
        // Arrange
        var response = new GetRecipesResponse([
            new RecipeListItemResponse(2, "soup", 4),
            new RecipeListItemResponse(1, "Pancakes", 2),
            new RecipeListItemResponse(3, "Apple pie", 6)
        ]);

        // Act
        var result = _sut.MapPanelRecipes(response);

        // Assert
        result.Should().Equal(
            new PanelRecipe(3, "Apple pie", 6),
            new PanelRecipe(1, "Pancakes", 2),
            new PanelRecipe(2, "soup", 4));
    }

    [Fact]
    public void MapPanelRecipes_ReturnsAnEmptyList_ForANullResponse()
    {
        // Act
        var result = _sut.MapPanelRecipes(null);

        // Assert
        result.Should().BeEmpty();
    }
}
