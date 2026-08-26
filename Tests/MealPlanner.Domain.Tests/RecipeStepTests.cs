using AwesomeAssertions;
using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.Shared;
using MealPlanner.Tests.Shared.Helpers;

namespace MealPlanner.Domain.Tests;

public class RecipeStepTests
{
    private const string Instructions = "Bake in 180 degrees for 45 minutes or until golden";
    
    [Theory]
    [ClassData(typeof(NegativeNumbersTestDataProvider))]
    public void Create_WithNegativeOrder_Fails(int invalidOrder)
    {
        // Act
        var result = RecipeStep.Create(invalidOrder, Instructions);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.RecipeStep.InvalidOrder(invalidOrder));
    }
    
    [Theory]
    [ClassData(typeof(EmptyStringTestDataProvider))]
    public void Create_WithEmptyInstructions_Fails(string instructions)
    {
        // Act
        var result = RecipeStep.Create(1, instructions);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.RecipeStep.InvalidInstruction(instructions));
    }

    [Fact]
    public void Create_WithPositiveOrderAndInstructions_Succeeds()
    {
        // Arrange
        const int expectedOrder = 15;
        
        // Act
        var result = RecipeStep.Create(expectedOrder, Instructions);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeFalse();
        result.Value.Order.Should().Be(expectedOrder);
        result.Value.Instructions.Should().Be(Instructions);
    }

    [Theory]
    [ClassData(typeof(NegativeNumbersTestDataProvider))]
    public void UpdateOrder_WithNegativeOrder_Fails(int newOrder)
    {
        // Arrange
        var step = RecipeStep.Create(3, Instructions).Value;
        
        // Act
        var result = step.UpdateOrder(newOrder);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.RecipeStep.InvalidOrder(newOrder));
    }

    [Fact]
    public void UpdateOrder_WithPositiveOrder_Succeeds()
    {
        // Arrange
        var step = RecipeStep.Create(3, Instructions).Value;
        const int newOrder = 4;
        
        // Act
        var result = step.UpdateOrder(newOrder);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeFalse();
        step.Order.Should().Be(newOrder);
    }

    [Theory]
    [ClassData(typeof(EmptyStringTestDataProvider))]
    public void UpdateInstructions_WithEmptyInstructions_Fails(string newInstructions)
    {
        // Arrange
        var step = RecipeStep.Create(1, Instructions).Value;
        
        // Act
        var result = step.UpdateInstructions(newInstructions);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.RecipeStep.InvalidInstruction(newInstructions));
    }

    [Fact]
    public void UpdateInstructions_WithInstructions_Succeeds()
    {
        // Arrange
        var step = RecipeStep.Create(1, Instructions).Value;
        const string newInstructions = "Completely new and previously unheard of instructions";
        
        // Act
        var result = step.UpdateInstructions(newInstructions);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeFalse();
        step.Instructions.Should().Be(newInstructions);
    }
}