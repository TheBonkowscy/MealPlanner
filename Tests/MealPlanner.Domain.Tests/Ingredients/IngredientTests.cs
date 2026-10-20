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
    public void Create_WithEmptyName_Fails(string ingredientName)
    {
        // Act
        var result = Ingredient.Create(ingredientName, Units);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Ingredient.InvalidName(ingredientName));
    }
    
    [Fact]
    public void Create_WithEmptyUnits_Fails()
    {
        // Act
        var result = Ingredient.Create(Name, []);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Ingredient.MissingMeasureUnits(Name));
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

    [Fact]
    public void UpdateApplicableUnits_WithEmptyUnits_Fails()
    {
        // Arrange
        var ingredient = Ingredient.Create(Name, Units).Value;
        var expectedError = DomainErrors.Ingredient.MissingMeasureUnits(ingredient.Name);
        
        // Act
        var result = ingredient.UpdateApplicableUnits([]);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(expectedError.Code);
        result.Error.Type.Should().Be(expectedError.Type);
        result.Error.Metadata.Should().BeEquivalentTo(expectedError.Metadata);
    }

    [Fact]
    public void UpdateApplicableUnits_WithUnits_ReplacesExistingUnits()
    {
        // Arrange
        var ingredient = Ingredient.Create(Name, Units).Value;
        var previousUnits = ingredient.ApplicableUnits;
        var newUnits = Enum.GetValues<MeasureUnit>().Except(ingredient.ApplicableUnits).ToList();
        
        // Act
        var result = ingredient.UpdateApplicableUnits(newUnits);
        
        // Assert
        result.IsFailure.Should().BeFalse();
        newUnits.Should().Contain(newUnits);
        newUnits.Should().NotContain(previousUnits);
    }
}