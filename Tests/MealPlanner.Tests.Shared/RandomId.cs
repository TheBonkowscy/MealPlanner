using MealPlanner.Domain;
using MealPlanner.Domain.Ingredients;

namespace MealPlanner.Tests.Shared;

public static class RandomId
{
    public static void Set(params Menu[] menus)
    {
        foreach(var menu in menus)
        {
            var field = typeof(Menu).GetProperty(nameof(Menu.Id));
            field!.SetValue(menu, Random.Shared.Next(1, 1000));
        }
    }

    public static void Set(params Recipe[] recipes)
    {
        foreach(var recipe in recipes)
        {
            var field = typeof(Recipe).GetProperty(nameof(Recipe.Id));
            field!.SetValue(recipe, Random.Shared.Next(1, 1000));
        }
    }

    public static void Set(params MeasureUnit[] measureUnits)
    {
        foreach(var measureUnit in measureUnits)
        {
            var field = typeof(MeasureUnit).GetProperty(nameof(MeasureUnit.Id));
            field!.SetValue(measureUnit, Random.Shared.Next(1, 1000));
        }
    }
}