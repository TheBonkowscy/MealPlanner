using MealPlanner.Domain;

namespace MealPlanner.Tests.Shared.Factories;

public static class TestMenu
{
    public static Menu Create(DateOnly date)
    {
        return Create(date, [CreateMeal()]);
    }

    public static Menu Create(DateOnly date, List<Meal> meal)
    {
        return Menu.Create(date, meal);
    }

    private static Meal CreateMeal(string? name = null)
    {
        var randomMeal = Meal.Create(Guid.NewGuid().ToString());
        return randomMeal;
    }
}