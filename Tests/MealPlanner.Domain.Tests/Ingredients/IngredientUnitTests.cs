using AwesomeAssertions;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Tests.Shared.Helpers;

namespace MealPlanner.Domain.Tests.Ingredients;

public class IngredientUnitTests
{
    private const string Name = "Cups";
    
    [Theory]
    [ClassData(typeof(EmptyStringTestDataProvider))]
    public void Create_WithEmptyName_Throws(string ingredientName)
    {
        // Act
        Action<string> create = name => IngredientUnit.Create(name);
        
        // Assert
        create.Invoking(x => x.Invoke(ingredientName))
            .Should().Throw<ArgumentNullException>()
            .WithMessage("Name cannot be null or whitespace");
    }

    [Fact]
    public void Create_WithNonEmptyName_Succeeds()
    {
        // Act
        var unit = IngredientUnit.Create(Name);
        
        // Assert
        unit.Should().NotBeNull();
        unit.Name.Should().Be(Name);
        
    }
}