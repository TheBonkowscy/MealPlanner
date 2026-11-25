namespace MealPlanner.UI.Services;

public static class WeekNavigation
{
    private const int DaysInWeek = 7;

    public static DateOnly StartOf(DateOnly date) => date.AddDays(-DaysSinceMonday(date));

    public static DateOnly Previous(DateOnly date) => StartOf(date).AddDays(-DaysInWeek);

    public static DateOnly Next(DateOnly date) => StartOf(date).AddDays(DaysInWeek);

    private static int DaysSinceMonday(DateOnly date) => ((int)date.DayOfWeek + DaysInWeek - 1) % DaysInWeek;
}
