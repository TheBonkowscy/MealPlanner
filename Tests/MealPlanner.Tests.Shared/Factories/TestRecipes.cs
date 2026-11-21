using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Recipes;

namespace MealPlanner.Tests.Shared.Factories;

public static class TestRecipes
{
    public static Recipe Create(string? name = null, 
        int? servings = null,
        List<Ingredient>? ingredients = null, 
        List<RecipeStep>? steps = null,
        List<RecipePreparation>? preparations = null)
    {
        name ??= $"Recipe_{Guid.NewGuid().ToString()}";
        servings ??= 1;
        ingredients ??= [TestIngredients.Create()];
        steps ??= [TestRecipeSteps.Create()];
        preparations ??= [TestRecipePreparation.Create()];
        
        var addIngredients = ingredients.Select(i => TestActions.AddIngredient(i, 1, i.ApplicableUnits.First())).ToList();

        var randomRecipe = Recipe.Create(name, servings.Value, addIngredients, steps, preparations).Value;
        return randomRecipe;
    }

    // Menu meals copy Recipe.Id into Meal.RecipeId when they are created, so the id has to be set before
    // the recipe is added to a menu. Unlike RandomId.Set it is deterministic, so two recipes never collide.
    public static Recipe CreateWithId(int id, string? name = null)
    {
        var recipe = Create(name);
        typeof(Recipe).GetProperty(nameof(Recipe.Id))!.SetValue(recipe, id);
        return recipe;
    }
}