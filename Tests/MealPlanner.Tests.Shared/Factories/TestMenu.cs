using MealPlanner.Domain;

namespace MealPlanner.Tests.Shared.Factories;

public static class TestMenu
{
    public static Menu Create(DateOnly date)
    {
        return Create(date, [TestRecipes.Create()]);
    }

    public static Menu Create(DateOnly date, List<Recipe> recipes)
    {
        return Menu.Create(date, recipes);
    }
}