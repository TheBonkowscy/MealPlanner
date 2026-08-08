using AwesomeAssertions;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Ingredients.Actions;

namespace MealPlanner.Domain.Tests.Ingredients;

public class UsedIngredientTests
{
    private const string IngredientName = "Flour";
    private static readonly IngredientUnit Unit = IngredientUnit.Create("Cups");
    private static readonly Ingredient Ingredient = Ingredient.Create(IngredientName, [Unit]);

    [Fact]
    public void Create_FromAction_Succeeds()
    {
        // Arrange
        var meal = Recipe.Create("Test Meal");
        var action = AddIngredientAction.Create(Ingredient, 0.75m, Unit);
        
        // Act
        var result = UsedIngredient.Create(meal, action);
        
        // Assert
        result.Should().NotBeNull();
        result.Recipe.Should().Be(meal);
        result.Ingredient.Should().Be(action.Ingredient);
        result.Unit.Should().Be(action.Unit);
        result.Quantity.Should().Be(action.Quantity);
    }
}