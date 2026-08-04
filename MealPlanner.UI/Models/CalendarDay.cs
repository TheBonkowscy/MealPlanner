namespace MealPlanner.UI.Models;

public sealed class CalendarDay
{
    public int? Id { get; set; }
    public DateOnly Day { get; set; }
    public bool Exists { get; set; }
}