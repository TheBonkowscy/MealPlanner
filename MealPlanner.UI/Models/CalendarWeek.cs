namespace MealPlanner.UI.Models;

public sealed class CalendarWeek
{
    public int WeekNumber { get; set; }
    public int Year { get; set; }

    public List<CalendarDay> Days { get; set; } = [];
        
    public bool Current => Days.Any(x => x.Day == DateOnly.FromDateTime(DateTime.Today));
}