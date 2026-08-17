using MealPlanner.Domain;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Menus;

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

    public static void Set(params Ingredient[] ingredients)
    {
        foreach(var ingredient in ingredients)
        {
            var field = typeof(Ingredient).GetProperty(nameof(Ingredient.Id));
            field!.SetValue(ingredient, Random.Shared.Next(1, 1000));
        }
    }

    public static void Set(params UsedIngredient[] ingredients)
    {
        foreach(var ingredient in ingredients)
        {
            var idProp = typeof(UsedIngredient).GetProperty(nameof(UsedIngredient.IngredientId));
            var underlyingIngredientProp = typeof(UsedIngredient).GetProperty(nameof(UsedIngredient.Ingredient));
            var underlyingIngredient = (underlyingIngredientProp!.GetValue(ingredient)) as Ingredient ?? throw new InvalidOperationException("No property!");
            idProp!.SetValue(ingredient, underlyingIngredient.Id);
        }
    }

    public static void Set(params RecipeStep[] recipeSteps)
    {
        foreach(var recipeStep in recipeSteps)
        {
            var field = typeof(RecipeStep).GetProperty(nameof(RecipeStep.Id));
            field!.SetValue(recipeStep, Random.Shared.Next(1, 1000));
        }
    }
}