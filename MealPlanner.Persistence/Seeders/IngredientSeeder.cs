using MealPlanner.Domain.Ingredients;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Persistence.Seeders;

public static class IngredientSeeder
{
    public static async Task Seed(DbContext context, CancellationToken cancellationToken)
    {
        var seedData = InitialData.Ingredients();
        var allNames = seedData.Select(i => i.Name.ToLower()).Distinct().ToList();
        var existingIngredients = await context
            .Set<Ingredient>()
            .Where(x => allNames.Contains(x.Name.ToLower()))
            .ToDictionaryAsync(x => x.Name, x => x, cancellationToken);

        allNames.ForEach(name =>
        {
            var ingredient = seedData.First(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            var existingIngredient = existingIngredients.GetValueOrDefault(name);
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