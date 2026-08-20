using MealPlanner.Domain;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Recipes;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Persistence.Seeders;

internal static class RecipeSeeder
{
    internal static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        var ingredients = await context.Set<Ingredient>().ToArrayAsync(cancellationToken);
        var seedData = InitialData.Recipes(ingredients);
        var allNames = seedData.Select(i => i.Name.ToLower()).Distinct().ToList();
        var existingRecipes = await GetExistingRecipesAsync(context, allNames, cancellationToken);

        UpdateOrAddRecipe(context, allNames, seedData, existingRecipes);
        
        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task<Dictionary<string, Recipe>> GetExistingRecipesAsync(DbContext context, List<string> allNames, CancellationToken cancellationToken) =>
        await context
            .Set<Recipe>()
            .Where(x => allNames.Contains(x.Name.ToLower()))
            .ToDictionaryAsync(x => x.Name.ToLower(), x => x, cancellationToken);

    private static void UpdateOrAddRecipe(DbContext context, List<string> allNames, Recipe[] seedData, Dictionary<string, Recipe> existingRecipes) =>
        allNames.ForEach(name =>
        {
            var recipe = seedData.First(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            var existingRecipe = existingRecipes.GetValueOrDefault(name);
            if (existingRecipe != null)
            {
                context.Set<Recipe>().Remove(existingRecipe);
            }
            
            context.Set<Recipe>().Add(recipe);
        });

    internal static void Seed(DbContext context)
    {
        var ingredients = context.Set<Ingredient>().ToArray();
        var seedData = InitialData.Recipes(ingredients);
        var allNames = seedData.Select(i => i.Name.ToLower()).Distinct().ToList();
        var existingRecipes = GetExistingRecipes(context, allNames);

        UpdateOrAddRecipe(context, allNames, seedData, existingRecipes);
        
        context.SaveChanges();
    }

    private static Dictionary<string, Recipe> GetExistingRecipes(DbContext context, List<string> allNames) =>
        context
            .Set<Recipe>()
            .Where(x => allNames.Contains(x.Name.ToLower()))
            .ToDictionary(x => x.Name, x => x);
}