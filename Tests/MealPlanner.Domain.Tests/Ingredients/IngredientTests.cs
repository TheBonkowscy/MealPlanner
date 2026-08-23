using AwesomeAssertions;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Shared;
using MealPlanner.Tests.Shared.Helpers;

namespace MealPlanner.Domain.Tests.Ingredients;

public class IngredientTests
{
    private const string Name = "Flour";
    private static readonly List<MeasureUnit> Units = [MeasureUnit.GlassCup];
    
    [Theory]
    [ClassData(typeof(EmptyStringTestDataProvider))]
    public void Create_WithEmptyName_Throws(string ingredientName)
    {
        // Act
        var result = Ingredient.Create(ingredientName, Units);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Ingredient.InvalidName(ingredientName));
    }
    
    [Fact]
    public void Create_WithEmptyUnits_Throws()
    {
        // Act
        var result = Ingredient.Create(Name, []);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Ingredient.MissingMeasureUnits);
    }
    
    [Fact]
    public void Create_WithNameAndUnits_Succeeds()
    {
        // Act
        var result = Ingredient.Create(Name, Units);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeFalse();
        result.Value.Name.Should().Be(Name);
        result.Value.ApplicableUnits.Should().BeEquivalentTo(Units);
    }
    
    [Theory]
    [MemberData(nameof(IsApplicableTestData))]
    public void IsApplicable_ReturnsCorrectly(MeasureUnit unit, bool expectedResult)
    {
        // Arrange
        var ingredient = Ingredient.Create(Name, Units);
        
        // Act
        var result = ingredient.Value.IsApplicableUnit(unit);
        
        // Assert
        result.Should().Be(expectedResult);
    }

    public static IEnumerable<object[]> IsApplicableTestData()
    {
        yield return [MeasureUnit.GlassCup, true];
        yield return [MeasureUnit.Milliliter, false];
    }
}