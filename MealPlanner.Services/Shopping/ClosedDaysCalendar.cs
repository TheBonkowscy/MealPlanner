namespace MealPlanner.Services.Shopping;

// Days on which shopping is impossible: public holidays and non-trading Sundays.
public static class ClosedDaysCalendar
{
    public static readonly IReadOnlyList<DateOnly> Dates =
    [
        // TODO: finish filling
        new(2026, 11, 11),
        new(2026, 12, 24),
        new(2026, 12, 25),
        new(2026, 12, 26),
        new(2026, 12, 27)
    ];
}
