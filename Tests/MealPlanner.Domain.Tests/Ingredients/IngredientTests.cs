using AwesomeAssertions;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Tests.Shared;
using MealPlanner.Tests.Shared.Factories;
using MealPlanner.Tests.Shared.Helpers;

namespace MealPlanner.Domain.Tests.Ingredients;

public class IngredientTests
{
    private const string Name = "Flour";
    private static readonly List<MeasureUnit> Units = [TestIngredientsUnits.Cups()];
    
    [Theory]
    [ClassData(typeof(EmptyStringTestDataProvider))]
    public void Create_WithEmptyName_Throws(string ingredientName)
    {
        // Act
        Action<string, List<MeasureUnit>> create = (name, units) => Ingredient.Create(name, units);
        
        // Assert
        create.Invoking(c => c.Invoke(ingredientName, Units))
            .Should().Throw<ArgumentNullException>()
            .WithMessage("Ingredient name cannot be null or whitespace");
    }
    
    [Fact]
    public void Create_WithEmptyUnits_Throws()
    {
        // Act
        Action<string, List<MeasureUnit>> create = (name, units) => Ingredient.Create(name, units);
        
        // Assert
        create.Invoking(c => c.Invoke(Name, []))
            .Should().Throw<ArgumentException>()
            .WithMessage("Ingredient must have at least one applicable unit");
    }
    
    [Fact]
    public void Create_WithNameAndUnits_Succeeds()
    {
        // Act
        var result = Ingredient.Create(Name, Units);
        
        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(Name);
        result.ApplicableUnits.Should().BeEquivalentTo(Units);
    }
    
    [Theory]
    [MemberData(nameof(IsApplicableTestData))]
    public void IsApplicable_ReturnsCorrectly(MeasureUnit unit, bool expectedResult)
    {
        // Arrange
        var ingredient = Ingredient.Create(Name, Units);
        
        // Act
        var result = ingredient.IsApplicableUnit(unit);
        
        // Assert
        result.Should().Be(expectedResult);
    }

    public static IEnumerable<object[]> IsApplicableTestData()
    {
        yield return [TestIngredientsUnits.Cups(), true];
        var teaspoons = MeasureUnit.Create("Teaspoons");
        RandomId.Set(teaspoons);
        yield return [teaspoons, false];
    }
}