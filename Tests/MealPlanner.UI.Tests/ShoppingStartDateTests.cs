using AwesomeAssertions;
using MealPlanner.UI.Services;

namespace MealPlanner.UI.Tests;

public class ShoppingStartDateTests
{
    [Theory]
    [InlineData(2026, 10, 23)] // Friday
    [InlineData(2026, 10, 24)] // Saturday
    [InlineData(2026, 10, 25)] // Sunday
    public void Default_IsTodayWhateverItsWeekday(int year, int month, int day)
    {
        var today = new DateOnly(year, month, day);
        var timeProvider = new FixedTimeProvider(new DateTimeOffset(today.ToDateTime(new TimeOnly(10, 30)), TimeSpan.Zero));

        var result = ShoppingStartDate.Default(timeProvider);

        result.Should().Be(today);
    }

    [Fact]
    public void Default_UsesTheLocalDateNotTheUtcDate()
    {
        var timeZone = TimeZoneInfo.CreateCustomTimeZone("UTC+2", TimeSpan.FromHours(2), "UTC+2", "UTC+2");
        // 23:30 UTC on Friday is already 01:30 on Saturday in UTC+2.
        var timeProvider = new FixedTimeProvider(new DateTimeOffset(2026, 10, 23, 23, 30, 0, TimeSpan.Zero), timeZone);

        var result = ShoppingStartDate.Default(timeProvider);

        result.Should().Be(new DateOnly(2026, 10, 24));
    }

    private sealed class FixedTimeProvider(DateTimeOffset utcNow, TimeZoneInfo? timeZone = null) : TimeProvider
    {
        public override TimeZoneInfo LocalTimeZone { get; } = timeZone ?? TimeZoneInfo.Utc;

        public override DateTimeOffset GetUtcNow() => utcNow;
    }
}
