namespace MealPlanner.UI.Services;

public static class ShoppingStartDate
{
    /// <summary>
    /// The start day proposed on the shopping list page: today's local date, whatever its weekday.
    /// </summary>
    public static DateOnly Default(TimeProvider timeProvider) =>
        DateOnly.FromDateTime(timeProvider.GetLocalNow().Date);
}
