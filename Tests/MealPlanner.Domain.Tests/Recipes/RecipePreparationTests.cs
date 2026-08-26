using AwesomeAssertions;
using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.Shared;
using MealPlanner.Tests.Shared.Helpers;

namespace MealPlanner.Domain.Tests.Recipes;

public class RecipePreparationTests
{
    private const string Description = "Test description";
    
    [Theory]
    [ClassData(typeof(NegativeNumbersTestDataProvider))]
    public void Create_WithNegativeLeadDays_Fails(int invalidLeadDays)
    {
        // Act
        var result = RecipePreparation.Create(Description, invalidLeadDays, true);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.RecipePreparation.InvalidLeadDays(invalidLeadDays));
    }
    
    [Theory]
    [ClassData(typeof(EmptyStringTestDataProvider))]
    public void Create_WithEmptyDescription_Fails(string description)
    {
        // Act
        var result = RecipePreparation.Create(description, 1, true);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.RecipePreparation.InvalidDescription(description));
    }

    [Fact]
    public void Create_WithPositiveLeadDaysAndDescription_Succeeds()
    {
        // Arrange
        const int expectedLeadDays = 15;
        
        // Act
        var result = RecipePreparation.Create(Description, expectedLeadDays, true);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeFalse();
        result.Value.LeadDays.Should().Be(expectedLeadDays);
        result.Value.Description.Should().Be(Description);
    }
}