using AwesomeAssertions;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Ingredients.Actions;

namespace MealPlanner.Domain.Tests.Ingredients;

public class UsedIngredientTests
{
    private const string IngredientName = "Flour";
    private static readonly MeasureUnit Unit = MeasureUnit.Create("Cups");
    private static readonly Ingredient Ingredient = Ingredient.Create(IngredientName, [Unit]);

    [Fact]
    public void Create_FromAction_Succeeds()
    {
        // Arrange
        var recipe = Recipe.Create("Test Recipe");
        var action = AddIngredientAction.Create(Ingredient, 0.75m, Unit);
        
        // Act
        var result = UsedIngredient.Create(recipe, action);
        
        // Assert
        result.Should().NotBeNull();
        result.Recipe.Should().Be(recipe);
        result.Ingredient.Should().Be(action.Ingredient);
        result.Unit.Should().Be(action.Unit);
        result.Quantity.Should().Be(action.Quantity);
    }
}