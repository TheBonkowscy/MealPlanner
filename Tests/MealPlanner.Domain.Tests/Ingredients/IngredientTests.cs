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
    
    [Fact]
    public void Create_WithoutShelfLife_SetsShelfLifeToNull()
    {
        // Act
        var result = Ingredient.Create(Name, Units);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeFalse();
        result.Value.Name.Should().Be(Name);
        result.Value.ApplicableUnits.Should().BeEquivalentTo(Units);
        result.Value.ShelfLifeDays.Should().BeNull();
    }

    [Theory]
    [ClassData(typeof(EmptyStringTestDataProvider))]
    public void Create_WithShelfLifeAndEmptyName_Fails(string ingredientName)
    {
        // Act
        var result = Ingredient.Create(ingredientName, 3, Units);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Ingredient.InvalidName(ingredientName));
    }

    [Fact]
    public void Create_WithShelfLifeAndEmptyUnits_Fails()
    {
        // Act
        var result = Ingredient.Create(Name, 3, []);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Ingredient.MissingMeasureUnits(Name));
    }

    [Theory]
    [ClassData(typeof(NegativeNumbersTestDataProvider))]
    public void Create_WithNegativeShelfLife_Fails(int shelfLife)
    {
        // Act
        var result = Ingredient.Create(Name, shelfLife, []);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Ingredient.InvalidShelfLife(Name));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void Create_WithValidShelfLife_Succeeds(int shelfLife)
    {
        // Act
        var result = Ingredient.Create(Name, shelfLife, Units);
        
        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.ShelfLifeDays.Should().Be(shelfLife);
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
        ingredient.ApplicableUnits.Should().Contain(newUnits);
        ingredient.ApplicableUnits.Should().NotContain(previousUnits);
    }

    [Theory]
    [ClassData(typeof(NegativeNumbersTestDataProvider))]
    public void UpdateShelfLife_WithNegativeValue_Fails(int newShelfLife)
    {
        // Arrange
        var ingredient = Ingredient.Create(Name, 3, Units).Value;
        
        // Act
        var result = ingredient.UpdateShelfLife(newShelfLife);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeEquivalentTo(DomainErrors.Ingredient.InvalidShelfLife(ingredient.Name));
        ingredient.ShelfLifeDays.Should().Be(3);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void UpdateShelfLife_WithValidValue_SetsShelfLife(int newShelfLife)
    {
        // Arrange
        var ingredient = Ingredient.Create(Name, 3, Units).Value;
        
        // Act
        var result = ingredient.UpdateShelfLife(newShelfLife);
        
        // Assert
        result.IsFailure.Should().BeFalse();
        ingredient.ShelfLifeDays.Should().Be(newShelfLife);
    }

    [Fact]
    public void UpdateShelfLife_WithNull_ClearsShelfLife()
    {
        // Arrange
        var ingredient = Ingredient.Create(Name, 3, Units).Value;
        
        // Act
        var result = ingredient.UpdateShelfLife(null);
        
        // Assert
        result.IsFailure.Should().BeFalse();
        ingredient.ShelfLifeDays.Should().Be(null);
    }
}