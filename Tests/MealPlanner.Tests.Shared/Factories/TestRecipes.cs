using MealPlanner.Domain;
using MealPlanner.Domain.Ingredients;

namespace MealPlanner.Tests.Shared.Factories;

public static class TestRecipes
{
    public static Recipe Create(string? name = null, 
        List<Ingredient>? ingredients = null, 
        List<RecipeStep>? steps = null)
    {
        name ??= $"Recipe_{Guid.NewGuid().ToString()}";
        ingredients ??= [TestIngredients.Create()];
        steps ??= [TestRecipeSteps.Create()];
        
        var addIngredients = ingredients.Select(i => TestActions.AddIngredient(i, 1, i.ApplicableUnits.First())).ToList();

        var randomRecipe = Recipe.Create(name, addIngredients, steps);
        return randomRecipe;
    }
}