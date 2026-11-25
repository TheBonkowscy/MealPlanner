using System.Globalization;
using AwesomeAssertions;
using MealPlanner.UI.Services;

namespace MealPlanner.UI.Tests;

public class WeekNavigationTests
{
    [Theory]
    [InlineData("2026-11-23", "2026-11-23")]
    [InlineData("2026-11-24", "2026-11-23")]
    [InlineData("2026-11-25", "2026-11-23")]
    [InlineData("2026-11-26", "2026-11-23")]
    [InlineData("2026-11-27", "2026-11-23")]
    [InlineData("2026-11-28", "2026-11-23")]
    [InlineData("2026-11-29", "2026-11-23")]
    public void StartOf_ReturnsTheMondayOfTheWeekForEveryDayOfTheWeek(string date, string expectedMonday)
    {
        // Arrange
        var day = Day(date);

        // Act
        var result = WeekNavigation.StartOf(day);

        // Assert
        result.Should().Be(Day(expectedMonday));
        result.DayOfWeek.Should().Be(DayOfWeek.Monday);
    }

    [Theory]
    [InlineData("2026-12-28", "2026-12-28")]
    [InlineData("2026-12-31", "2026-12-28")]
    [InlineData("2027-01-01", "2026-12-28")]
    [InlineData("2027-01-03", "2026-12-28")]
    [InlineData("2027-01-04", "2027-01-04")]
    public void StartOf_FollowsTheIsoWeekAcrossTheYearBoundary(string date, string expectedMonday)
    {
        // Arrange
        var day = Day(date);

        // Act
        var result = WeekNavigation.StartOf(day);

        // Assert
        result.Should().Be(Day(expectedMonday));
    }

    [Theory]
    [InlineData("2026-11-23", "2026-11-16")]
    [InlineData("2026-11-26", "2026-11-16")]
    [InlineData("2026-11-29", "2026-11-16")]
    [InlineData("2027-01-04", "2026-12-28")]
    [InlineData("2027-01-01", "2026-12-21")]
    public void Previous_ReturnsTheMondayOfThePreviousWeek(string date, string expectedMonday)
    {
        // Arrange
        var day = Day(date);

        // Act
        var result = WeekNavigation.Previous(day);

        // Assert
        result.Should().Be(Day(expectedMonday));
    }

    [Theory]
    [InlineData("2026-11-23", "2026-11-30")]
    [InlineData("2026-11-26", "2026-11-30")]
    [InlineData("2026-11-29", "2026-11-30")]
    [InlineData("2026-12-28", "2027-01-04")]
    [InlineData("2027-01-03", "2027-01-04")]
    public void Next_ReturnsTheMondayOfTheNextWeek(string date, string expectedMonday)
    {
        // Arrange
        var day = Day(date);

        // Act
        var result = WeekNavigation.Next(day);

        // Assert
        result.Should().Be(Day(expectedMonday));
    }

    [Theory]
    [InlineData("2026-11-23")]
    [InlineData("2026-11-26")]
    [InlineData("2026-12-31")]
    [InlineData("2027-01-01")]
    public void NextOfPrevious_ReturnsTheStartOfTheSameWeek(string date)
    {
        // Arrange
        var day = Day(date);

        // Act
        var result = WeekNavigation.Next(WeekNavigation.Previous(day));

        // Assert
        result.Should().Be(WeekNavigation.StartOf(day));
    }

    private static DateOnly Day(string value) => DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
}
