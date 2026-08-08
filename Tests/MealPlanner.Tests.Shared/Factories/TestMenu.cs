using MealPlanner.Domain;

namespace MealPlanner.Tests.Shared.Factories;

public static class TestMenu
{
    public static Menu Create(DateOnly date)
    {
        return Create(date, [CreateMeal()]);
    }

    public static Menu Create(DateOnly date, List<Recipe> meal)
    {
        return Menu.Create(date, meal);
    }

    private static Recipe CreateMeal(string? name = null)
    {
        var randomMeal = Recipe.Create(Guid.NewGuid().ToString());
        return randomMeal;
    }
}