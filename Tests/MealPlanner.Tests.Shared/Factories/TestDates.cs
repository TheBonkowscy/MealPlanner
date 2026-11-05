namespace MealPlanner.Tests.Shared.Factories;

public static class TestDates
{
    public static DateOnly Oct(int day) => new(2026, 10, day);
    
    public static DateOnly[] Oct(int[] days) => [.. days.Select(Oct)];
    
    public static DateOnly Dec(int day) => new(2026, 12, day);

}