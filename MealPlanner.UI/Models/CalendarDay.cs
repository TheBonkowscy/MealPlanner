namespace MealPlanner.UI.Models;

public sealed class CalendarDay
{
    public int? Id { get; set; }
    public DateOnly Day { get; set; }
    public bool HasMeals { get; set; }
}