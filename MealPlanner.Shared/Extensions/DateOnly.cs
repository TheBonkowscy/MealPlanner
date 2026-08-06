namespace MealPlanner.Shared.Extensions;

public static class DateOnlyExtensions
{
    extension(DateOnly day)
    {
        public bool IsInPast()
        {
            var now = DateOnly.FromDateTime(DateTime.Now);
            return day < now;
        }
    }
}