using AwesomeAssertions;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Ingredients.Actions;
using MealPlanner.Tests.Shared.Factories;

namespace MealPlanner.Domain.Tests.Ingredients;

public class UsedIngredientTests
{
    private const string IngredientName = "Flour";
    private static readonly Ingredient Ingredient = Ingredient.Create(IngredientName, [MeasureUnit.GlassCup]).Value;

    [Fact]
    public void Create_FromAction_Succeeds()
    {
        // Arrange
        var recipe = TestRecipes.Create("Test Recipe");
        var action = AddIngredientAction.Create(Ingredient, 0.75m, MeasureUnit.GlassCup).Value;
        
        // Act
        var result = UsedIngredient.Create(recipe, action);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeFalse();
        result.Value.Recipe.Should().Be(recipe);
        result.Value.Ingredient.Should().Be(action.Ingredient);
        result.Value.Unit.Should().Be(action.Unit);
        result.Value.Quantity.Should().Be(action.Quantity);
    }
}