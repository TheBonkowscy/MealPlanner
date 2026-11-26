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

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 2)]
    [InlineData(3, 4)]
    [InlineData(4, 5)]
    [InlineData(99, 5)]
    [InlineData(-5, 1)]
    public void DecideAdd_ReturnsAddAtTheDroppedPosition_ClampedToOneThroughCountPlusOne(int dropIndex, int expectedPosition)
    {
        // Arrange
        int[] mealsInDay = [10, 20, 30, 40];

        // Act
        var result = WeekDropDecision.DecideAdd(50, 4, Monday, dropIndex, mealsInDay);

        // Assert
        result.Should().Be(new WeekDropAction.AddMeal(Monday, 50, expectedPosition, 4));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(-1)]
    public void DecideAdd_ReturnsAddAtPositionOne_ForADayWithoutMeals(int dropIndex)
    {
        // Arrange
        int[] mealsInDay = [];

        // Act
        var result = WeekDropDecision.DecideAdd(50, 2, Tuesday, dropIndex, mealsInDay);

        // Assert
        result.Should().Be(new WeekDropAction.AddMeal(Tuesday, 50, 1, 2));
    }

    [Fact]
    public void DecideAdd_CarriesTheServingsOfTheRecipe()
    {
        // Arrange
        int[] mealsInDay = [10, 20, 30, 40];

        // Act
        var result = WeekDropDecision.DecideAdd(50, 7, Monday, 2, mealsInDay);

        // Assert
        result.Should().Be(new WeekDropAction.AddMeal(Monday, 50, 3, 7));
    }

    [Fact]
    public void DecideAdd_ReturnsAdd_WhenTheRecipeIsAlreadyInTheDay()
    {
        // Arrange
        int[] mealsInDay = [10, 20];

        // Act
        var result = WeekDropDecision.DecideAdd(10, 3, Monday, 1, mealsInDay);

        // Assert
        result.Should().Be(new WeekDropAction.AddMeal(Monday, 10, 2, 3));
    }
}
