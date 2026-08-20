using AwesomeAssertions;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Ingredients.Actions;
using MealPlanner.Domain.Ingredients.Exceptions;
using MealPlanner.Tests.Shared;
using MealPlanner.Tests.Shared.Factories;

namespace MealPlanner.Domain.Tests.Ingredients.Actions;

public class AddIngredientActionTests
{
    private const decimal SharedExpectedQuantity = 0.75m;
    
    [Fact]
    public void Create_WithNotApplicableUnit_Throws()
    {
        // Arrange
        var ingredientToAdd = TestInitialData.CupsOfFlour();
        
        // Act
        Action<Ingredient, decimal, MeasureUnit> create = (ingredient, quantity, unit) =>
            AddIngredientAction.Create(ingredient, quantity, unit);
        
        // Assert
        create.Invoking(x => x.Invoke(ingredientToAdd, SharedExpectedQuantity, MeasureUnit.Kilogram))
            .Should().Throw<InvalidOperationException>("Ingredient does not support the specified unit");
    }
    
    [Fact]
    public void Create_WithNegativeQuantity_Throws()
    {
        // Arrange
        var ingredientToAdd = TestInitialData.CupsOfFlour();
        
        // Act
        Action<Ingredient, decimal, MeasureUnit> create = (ingredient, quantity, unit) =>
            AddIngredientAction.Create(ingredient, quantity, unit);
        
        // Assert
        create.Invoking(x => x.Invoke(ingredientToAdd, -SharedExpectedQuantity, MeasureUnit.GlassCup))
            .Should().Throw<InvalidIngredientQuantityException>();
    }
    
    [Fact]
    public void Create_WithApplicableUnitAndPositiveQuantity_Succeeds()
    {
        // Arrange
        var ingredientToAdd = TestInitialData.CupsOfFlour();
        const MeasureUnit expectedUnit = MeasureUnit.GlassCup;
        
        // Act
        var result = AddIngredientAction.Create(ingredientToAdd, SharedExpectedQuantity, expectedUnit);
        
        // Assert
        result.Should().NotBeNull();
        result.Ingredient.Should().Be(ingredientToAdd);
        result.Quantity.Should().Be(SharedExpectedQuantity);
        result.Unit.Should().Be(expectedUnit);
    }
}