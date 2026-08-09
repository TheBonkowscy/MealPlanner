using MealPlanner.Domain.Ingredients;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Persistence.Seeders;

internal static class IngredientSeeder
{
    internal static async Task Seed(DbContext context, CancellationToken cancellationToken)
    {
        var seedData = InitialData.Ingredients();
        var allLowercaseNames = seedData.Select(i => i.Name.ToLower()).Distinct().ToList();
        var existingIngredients = await context
            .Set<Ingredient>()
            .Where(x => allLowercaseNames.Contains(x.Name.ToLower()))
            .ToDictionaryAsync(x => x.Name.ToLower(), x => x, cancellationToken);

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
        
        await context.SaveChangesAsync(cancellationToken);
    }
}