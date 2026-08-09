using MealPlanner.Domain;
using MealPlanner.Domain.Ingredients;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Persistence.Seeders;

internal static class RecipeSeeder
{
    internal static async Task Seed(DbContext context, CancellationToken cancellationToken)
    {
        var ingredients = await context.Set<Ingredient>().ToArrayAsync(cancellationToken);
        var seedData = InitialData.Recipes(ingredients);
        var allNames = seedData.Select(i => i.Name.ToLower()).Distinct().ToList();
        var existingRecipes = await context
            .Set<Recipe>()
            .Where(x => allNames.Contains(x.Name.ToLower()))
            .ToDictionaryAsync(x => x.Name, x => x, cancellationToken);

        allNames.ForEach(name =>
        {
            var recipe = seedData.First(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            var existingRecipe = existingRecipes.GetValueOrDefault(name);
            if (existingRecipe != null)
            {
                existingRecipe.UpdateIngredients([.. recipe.Ingredients]);
                existingRecipe.UpdateSteps([.. recipe.Steps]);
            }
            else
            {
                context.Set<Recipe>().Add(recipe);
            }
        });
        
        await context.SaveChangesAsync(cancellationToken);
    }
}