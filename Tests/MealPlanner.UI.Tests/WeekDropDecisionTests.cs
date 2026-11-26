using AwesomeAssertions;
using MealPlanner.UI.Services;

namespace MealPlanner.UI.Tests;

public class WeekDropDecisionTests
{
    private static readonly DateOnly Monday = new(2026, 11, 23);
    private static readonly DateOnly Tuesday = new(2026, 11, 24);

    [Theory]
    [InlineData(10, 2, 3)]
    [InlineData(20, 3, 4)]
    [InlineData(30, 1, 2)]
    [InlineData(40, 0, 1)]
    [InlineData(10, 99, 4)]
    [InlineData(40, -5, 1)]
    public void Decide_ReturnsReorderToTheDroppedPosition_ClampedToTheDay(int recipeId, int dropIndex, int expectedPosition)
    {
        // Arrange
        int[] mealsInDay = [10, 20, 30, 40];

        // Act
        var result = WeekDropDecision.Decide(Monday, recipeId, Monday, dropIndex, mealsInDay);

        // Assert
        result.Should().Be(new WeekDropAction.ReorderMeal(Monday, recipeId, expectedPosition));
    }

    [Theory]
    [InlineData(10, 0)]
    [InlineData(20, 1)]
    [InlineData(40, 3)]
    [InlineData(40, 99)]
    [InlineData(10, -5)]
    public void Decide_ReturnsNone_WhenTheMealStaysWhereItIs(int recipeId, int dropIndex)
    {
        // Arrange
        int[] mealsInDay = [10, 20, 30, 40];

        // Act
        var result = WeekDropDecision.Decide(Monday, recipeId, Monday, dropIndex, mealsInDay);

        // Assert
        result.Should().Be(WeekDropAction.None);
    }

    [Fact]
    public void Decide_ReturnsNone_WhenTheDropIsOnAnotherDay()
    {
        // Arrange
        int[] mealsInDay = [10, 20, 30, 40];

        // Act
        var result = WeekDropDecision.Decide(Monday, 10, Tuesday, 2, mealsInDay);

        // Assert
        result.Should().Be(WeekDropAction.None);
    }

    [Fact]
    public void Decide_ReturnsNone_WhenTheRecipeIsNotInTheDay()
    {
        // Arrange
        int[] mealsInDay = [10, 20, 30, 40];

        // Act
        var result = WeekDropDecision.Decide(Monday, 99, Monday, 0, mealsInDay);

        // Assert
        result.Should().Be(WeekDropAction.None);
    }

    [Fact]
    public void Decide_ReturnsNone_ForADayWithASingleMeal()
    {
        // Arrange
        int[] mealsInDay = [10];

        // Act
        var result = WeekDropDecision.Decide(Monday, 10, Monday, 5, mealsInDay);

        // Assert
        result.Should().Be(WeekDropAction.None);
    }
}
