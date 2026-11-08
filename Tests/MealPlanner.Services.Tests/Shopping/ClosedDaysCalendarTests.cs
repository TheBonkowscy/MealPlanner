using AwesomeAssertions;
using MealPlanner.Services.Shopping;

namespace MealPlanner.Services.Tests.Shopping;

public class ClosedDaysCalendarTests
{
    [Fact]
    public void Dates_ContainNoDuplicates()
    {
        ClosedDaysCalendar.Dates.Should().OnlyHaveUniqueItems();
    }
}
