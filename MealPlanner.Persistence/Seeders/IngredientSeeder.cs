using MealPlanner.Domain.Ingredients;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Persistence.Seeders;

internal static class IngredientSeeder
{
    internal static async Task SeedAsync(DbContext context, CancellationToken cancellationToken)
    {
        var seedData = InitialData.Ingredients();
        var allLowercaseNames = seedData.Select(i => i.Name.ToLower()).Distinct().ToList();
        var existingIngredients = await GetExistingIngredientsAsync(context, allLowercaseNames, cancellationToken);

        UpdateOrAddIngredient(context, allLowercaseNames, seedData, existingIngredients);
        
        await context.SaveChangesAsync(cancellationToken);
    }

    private static void UpdateOrAddIngredient(DbContext context, List<string> allLowercaseNames, Ingredient[] seedData,
        Dictionary<string, Ingredient> existingIngredients)
    {
        allLowercaseNames.ForEach(lowercaseName =>
        {
            var ingredient = seedData.First(x => x.Name.Equals(lowercaseName, StringComparison.CurrentCultureIgnoreCase));
            var existingIngredient = existingIngredients.GetValueOrDefault(lowercaseName);
            if (existingIngredient != null)
            {
                existingIngredient.UpdateApplicableUnits(ingredient.ApplicableUnits);
            }
            else
            {
                context.Set<Ingredient>().Add(ingredient);
            }
        });
    }

    private static async Task<Dictionary<string, Ingredient>> GetExistingIngredientsAsync(DbContext context,
        List<string> allLowercaseNames,
        CancellationToken cancellationToken) =>
        await context
            .Set<Ingredient>()
            .Where(x => allLowercaseNames.Contains(x.Name.ToLower()))
            .ToDictionaryAsync(x => x.Name.ToLower(), x => x, cancellationToken);

    internal static void Seed(DbContext context)
    {
        var seedData = InitialData.Ingredients();
        var allLowercaseNames = seedData.Select(i => i.Name.ToLower()).Distinct().ToList();
        var existingIngredients = GetExistingIngredients(context, allLowercaseNames, CancellationToken.None);

        UpdateOrAddIngredient(context, allLowercaseNames, seedData, existingIngredients);
        
        context.SaveChanges();
    }

    private static Dictionary<string, Ingredient> GetExistingIngredients(DbContext context,
        List<string> allLowercaseNames,
        CancellationToken cancellationToken) => 
        context
            .Set<Ingredient>()
            .Where(x => allLowercaseNames.Contains(x.Name.ToLower()))
            .ToDictionary(x => x.Name.ToLower(), x => x);
}