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

    public static void Set(Meal meal)
    {
        var field = typeof(Meal).GetProperty(nameof(Meal.Id));
        field!.SetValue(meal, Random.Shared.Next(1, 1000));
    }
}