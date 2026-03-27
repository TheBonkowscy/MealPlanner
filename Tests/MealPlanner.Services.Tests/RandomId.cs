using MealPlanner.Domain;

namespace MealPlanner.Services.Tests;

// NOTE: Should I be reusable?
public static class RandomId
{
    public static void Set(Menu menu)
    {
        var field = typeof(Menu).GetProperty(nameof(Menu.Id));
        field!.SetValue(menu, Random.Shared.Next(1, 1000));
    }
}