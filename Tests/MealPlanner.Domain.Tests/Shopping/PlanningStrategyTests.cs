using AwesomeAssertions;
using MealPlanner.Domain.Shopping.Planning;

namespace MealPlanner.Domain.Tests.Shopping;

public class PlanningStrategyTests
{
    private static DateOnly Oct(int day) => new(2026, 10, day);

    private static DateOnly[] Oct(int[] days) => [.. days.Select(Oct)];

    [Theory]
    [InlineData(new[] { 23, 24, 26 }, new int[0], 26)]
    [InlineData(new[] { 23, 24, 26 }, new[] { 23 }, 26)]
    [InlineData(new[] { 26 }, new int[0], 26)]
    public void LatestInWindow_ReturnsLatestAllowedTrip(int[] allowedDays, int[] usedDays, int expectedDay)
    {
        // Arrange
        var strategy = new LatestInWindowStrategy();

        // Act
        var chosen = strategy.Choose(Oct(allowedDays), Oct(usedDays).ToHashSet());

        // Assert
        chosen.Should().Be(Oct(expectedDay));
    }

    [Theory]
    [InlineData(new[] { 23, 24, 26 }, new[] { 23, 24 }, 24)]
    [InlineData(new[] { 23, 24, 26 }, new[] { 23 }, 23)]
    [InlineData(new[] { 23, 24, 26 }, new[] { 23, 24, 26 }, 26)]
    public void ReuseExistingTrip_ReturnsLatestUsedAllowedTrip(int[] allowedDays, int[] usedDays, int expectedDay)
    {
        // Arrange
        var strategy = new ReuseExistingTripStrategy();

        // Act
        var chosen = strategy.Choose(Oct(allowedDays), Oct(usedDays).ToHashSet());

        // Assert
        chosen.Should().Be(Oct(expectedDay));
    }

    [Theory]
    [InlineData(new[] { 23, 24, 26 }, new int[0], 26)]
    [InlineData(new[] { 23, 24, 26 }, new[] { 27 }, 26)] // the used trip is outside the allowed ones
    public void ReuseExistingTrip_WhenNoAllowedTripIsUsed_ReturnsLatestAllowedTrip(int[] allowedDays, int[] usedDays,
        int expectedDay)
    {
        // Arrange
        var strategy = new ReuseExistingTripStrategy();

        // Act
        var chosen = strategy.Choose(Oct(allowedDays), Oct(usedDays).ToHashSet());

        // Assert
        chosen.Should().Be(Oct(expectedDay));
    }
}
