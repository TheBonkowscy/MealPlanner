using MealPlanner.Domain.Menus;
using MealPlanner.Domain.Menus.Actions;

namespace MealPlanner.Tests.Shared.Factories;

public static class TestMenu
{
    public static Menu Create(DateOnly date)
    {
        var addIngredients = TestActions.AddMeal(TestRecipes.Create(), 1, 1);
        return Create(date, [addIngredients]);
    }

    public static Menu Create(DateOnly date, List<AddMealAction> mealsToAdd)
    {
        return Menu.Create(date, mealsToAdd);
    }
}