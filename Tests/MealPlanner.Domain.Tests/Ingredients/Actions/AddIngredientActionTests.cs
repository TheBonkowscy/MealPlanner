using AwesomeAssertions;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Ingredients.Actions;
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
        var result = AddIngredientAction.Create(ingredientToAdd, SharedExpectedQuantity, MeasureUnit.Kilogram);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Ingredients.UnitNotApplicable);
    }
    
    [Fact]
    public void Create_WithNegativeQuantity_Throws()
    {
        // Arrange
        var ingredientToAdd = TestInitialData.CupsOfFlour();
        
        // Act
        var result = AddIngredientAction.Create(ingredientToAdd, -SharedExpectedQuantity, MeasureUnit.GlassCup);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Ingredients.InvalidQuantity);
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
        result.IsFailure.Should().BeFalse();
        result.IsSuccess.Should().BeTrue();
        result.Value.Ingredient.Should().Be(ingredientToAdd);
        result.Value.Quantity.Should().Be(SharedExpectedQuantity);
        result.Value.Unit.Should().Be(expectedUnit);
    }
}