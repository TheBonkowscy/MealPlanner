using MealPlanner.Domain;

namespace MealPlanner.Tests.Shared.Factories;

public class TestRecipes
{
    public static Recipe Create(string? name = null)
    {
        var randomRecipe = Recipe.Create(name ?? Guid.NewGuid().ToString());
        return randomRecipe;
    }
}