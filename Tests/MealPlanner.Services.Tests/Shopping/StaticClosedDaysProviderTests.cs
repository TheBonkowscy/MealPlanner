using AwesomeAssertions;
using MealPlanner.Services.Shared;
using MealPlanner.Services.Shopping;
using MealPlanner.Tests.Shared.Factories;

namespace MealPlanner.Services.Tests.Shopping;

public class StaticClosedDaysProviderTests
{
    private static readonly DateOnly[] ClosedDays = TestDates.Oct([2, 5, 8]);

    [Theory]
    [MemberData(nameof(RangeCases))]
    public async Task GetClosedDays_ReturnsOnlyTheClosedDaysWithinTheRange(DateOnly from, DateOnly to, DateOnly[] expected)
    {
        // Arrange
        var sut = new StaticClosedDaysProvider(ClosedDays);

        // Act
        var result = await sut.GetClosedDays(from, to, CancellationToken.None);

        // Assert
        result.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetClosedDays_ReturnsNothing_WhenTheListIsEmpty()
    {
        // Arrange: the shipped list starts empty until the product owner adds holidays
        var sut = new StaticClosedDaysProvider([]);

        // Act
        var result = await sut.GetClosedDays(TestDates.Oct(1), TestDates.Oct(31), CancellationToken.None);

        // Assert
        result.Value.Should().BeEmpty();
    }

    [Theory]
    [MemberData(nameof(InvertedRanges))]
    public async Task GetClosedDays_Fails_WhenTheRangeIsInverted(DateOnly from, DateOnly to)
    {
        // Arrange
        var sut = new StaticClosedDaysProvider(ClosedDays);

        // Act
        var result = await sut.GetClosedDays(from, to, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(ServiceErrors.Shopping.InvertedDateRange(from, to));
    }

    public static TheoryData<DateOnly, DateOnly> InvertedRanges => new()
    {
        // from, to
        { TestDates.Oct(8), TestDates.Oct(2) }, // the range ends well before it starts
        { TestDates.Oct(6), TestDates.Oct(5) }, // the range ends one day before it starts
    };

    // The provider holds the closed days 2, 5 and 8 October. Expected values are written out by hand.
    public static TheoryData<DateOnly, DateOnly, DateOnly[]> RangeCases => new()
    {
        // from, to, expected
        { TestDates.Oct(2), TestDates.Oct(8), TestDates.Oct([2, 5, 8]) }, // a closed day on each bound is included
        { TestDates.Oct(3), TestDates.Oct(7), TestDates.Oct([5]) },       // closed days outside both bounds are left out
        { TestDates.Oct(5), TestDates.Oct(5), TestDates.Oct([5]) },       // a one-day range on a closed day
        { TestDates.Oct(6), TestDates.Oct(7), [] },                       // a range between closed days
        { TestDates.Oct(1), TestDates.Oct(1), [] },                       // every closed day is after the range
        { TestDates.Oct(9), TestDates.Oct(12), [] },                      // every closed day is before the range
    };
}
