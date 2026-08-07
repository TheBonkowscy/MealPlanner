using MealPlanner.Domain;
using MealPlanner.Domain.Ingredients;

namespace MealPlanner.Tests.Shared;

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

    public static void Set(params IngredientUnit[] ingredientUnits)
    {
        foreach(var ingredientUnit in ingredientUnits)
        {
            var field = typeof(IngredientUnit).GetProperty(nameof(IngredientUnit.Id));
            field!.SetValue(ingredientUnit, Random.Shared.Next(1, 1000));
        }
    }
}