using AwesomeAssertions;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Ingredients.Actions;
using MealPlanner.Tests.Shared;
using MealPlanner.Tests.Shared.Factories;

namespace MealPlanner.Domain.Tests.Ingredients.Actions;

public class AddIngredientActionTests
{
    private static readonly IngredientUnit NotApplicableUnit = IngredientUnit.Create("Pieces");
    private const decimal SharedExpectedQuantity = 0.75m;
    
    [Fact]
    public void Create_WithNotApplicableUnit_Throws()
    {
        // Arrange
        var ingredientToAdd = TestIngredients.CupsOfFlour();
        RandomId.Set([.. ingredientToAdd.ApplicableUnits]);
        
        // Act
        Action<Ingredient, decimal, IngredientUnit> create = (ingredient, quantity, unit) =>
            AddIngredientAction.Create(ingredient, quantity, unit);
        
        // Assert
        create.Invoking(x => x.Invoke(ingredientToAdd, SharedExpectedQuantity, NotApplicableUnit))
            .Should().Throw<InvalidOperationException>("Ingredient does not support the specified unit");
    }
    
    [Fact]
    public void Create_WithNegativeQuantity_Throws()
    {
        // Arrange
        var ingredientToAdd = TestIngredients.CupsOfFlour();
        
        // Act
        Action<Ingredient, decimal, IngredientUnit> create = (ingredient, quantity, unit) =>
            AddIngredientAction.Create(ingredient, quantity, unit);
        
        // Assert
        create.Invoking(x => x.Invoke(ingredientToAdd, -SharedExpectedQuantity, TestIngredientsUnits.Cups()))
            .Should().Throw<ArgumentOutOfRangeException>("Ingredient quantity must be positive");
    }
    
    [Fact]
    public void Create_WithApplicableUnitAndPositiveQuantity_Succeeds()
    {
        // Arrange
        var ingredientToAdd = TestIngredients.CupsOfFlour();
        var expectedUnit = TestIngredientsUnits.Cups();
        
        // Act
        var result = AddIngredientAction.Create(ingredientToAdd, SharedExpectedQuantity, expectedUnit);
        
        // Assert
        result.Should().NotBeNull();
        result.Ingredient.Should().Be(ingredientToAdd);
        result.Quantity.Should().Be(SharedExpectedQuantity);
        result.Unit.Should().Be(expectedUnit);
    }
}