namespace MealPlanner.Domain.Shopping;

// TODO: at some point this should be pulled from the database. Right now it lives here.
public static class ShoppingSchedule
{
    public const int CycleLengthInDays = 7;
    public const int MaxBigTripShiftInDays = 7;

    public static readonly DayOfWeek[] FreshShopDays =
    [
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday,
        DayOfWeek.Friday,
        DayOfWeek.Saturday
    ];
}