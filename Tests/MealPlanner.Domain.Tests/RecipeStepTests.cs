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
        var step = RecipeStep.Create(expectedOrder, Instructions);
        
        // Assert
        step.Should().NotBeNull();
        step.Order.Should().Be(expectedOrder);
        step.Instructions.Should().Be(Instructions);
    }

    [Theory]
    [ClassData(typeof(NegativeNumbersTestDataProvider))]
    public void UpdateOrder_WithNegativeOrder_Throws(int newOrder)
    {
        // Arrange
        var step = RecipeStep.Create(3, Instructions);
        
        // Act
        var update = () => step.UpdateOrder(newOrder);
        
        // Assert
        update.Should().Throw<ArgumentOutOfRangeException>("Order must be greater than 0");
    }

    [Fact]
    public void UpdateOrder_WithPositiveOrder_Succeeds()
    {
        // Arrange
        var step = RecipeStep.Create(3, Instructions);
        const int newOrder = 4;
        
        // Act
        step.UpdateOrder(newOrder);
        
        // Assert
        step.Order.Should().Be(newOrder);
    }

    [Theory]
    [ClassData(typeof(EmptyStringTestDataProvider))]
    public void UpdateInstructions_WithEmptyInstructions_Throws(string newInstructions)
    {
        // Arrange
        var step = RecipeStep.Create(1, Instructions);
        
        // Act
        var update = () => step.UpdateInstructions(newInstructions);
        
        // Assert
        update.Should().Throw<ArgumentNullException>("Instruction cannot be null or whitespace");
    }

    public void UpdateInstructions_WithInstructions_Succeeds()
    {
        
    }
}