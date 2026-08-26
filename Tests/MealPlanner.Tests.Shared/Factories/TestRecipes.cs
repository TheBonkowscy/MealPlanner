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
}