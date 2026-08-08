using AwesomeAssertions;
using MealPlanner.Tests.Shared.Helpers;

namespace MealPlanner.Domain.Tests;

public class RecipeStepTests
{
    private const string Instructions = "Bake in 180 degrees for 45 minutes or until golden";
    
    [Fact]
    public void Create_WithNegativeOrder_Throws()
    {
        // Act
        Action<int> create = order => RecipeStep.Create(order, Instructions);
        
        // Assert
        create.Invoking(x => x.Invoke(0))
            .Should().Throw<ArgumentOutOfRangeException>("Order must be greater than 0");
    }
    
    [Theory]
    [ClassData(typeof(EmptyStringTestDataProvider))]
    public void Create_WithEmptyInstructions_Throws(string instructions)
    {
        // Act
        Action<string> create = instruction => RecipeStep.Create(1, instruction);
        
        // Assert
        create.Invoking(x => x.Invoke(instructions))
            .Should().Throw<ArgumentNullException>()
            .WithMessage("Instruction cannot be null or whitespace");
    }

    [Fact]
    public void Create_WithPositiveOrderAndInstructions_Succeeds()
    {
        // Arrange
        const int expectedOrder = 15;
        
        // Act
        var unit = RecipeStep.Create(expectedOrder, Instructions);
        
        // Assert
        unit.Should().NotBeNull();
        unit.Order.Should().Be(expectedOrder);
        unit.Instructions.Should().Be(Instructions);
        
    }
}