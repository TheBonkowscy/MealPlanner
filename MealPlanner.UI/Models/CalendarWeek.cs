namespace MealPlanner.UI.Models;

public sealed class CalendarWeek
{
    public int WeekNumber { get; set; }

    public List<DateTime> Days { get; set; } = [];
        
    public bool Current => Days.Contains(DateTime.Today);
}