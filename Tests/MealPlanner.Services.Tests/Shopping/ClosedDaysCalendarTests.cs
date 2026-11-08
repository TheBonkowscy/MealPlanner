using AwesomeAssertions;
using MealPlanner.Services.Shopping;

namespace MealPlanner.Services.Tests.Shopping;

public class ClosedDaysCalendarTests
{
    [Fact]
    public void Dates_ContainNoDuplicates()
    {
        // The list is filled in by hand, so a repeated date usually means a typo for a different day
        ClosedDaysCalendar.Dates.Should().OnlyHaveUniqueItems();
    }
}
