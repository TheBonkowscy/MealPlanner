using MealPlanner.Domain;
using MealPlanner.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Recipes;

public interface IMapRecipes
{
    Task<List<Recipe>> MapRecipes(Dictionary<int, string> chosenMeals, CancellationToken ct);
}

public class RecipeMapper(MealPlannerDbContext ctx) : IMapRecipes
{
    public async Task<List<Recipe>> MapRecipes(Dictionary<int, string> chosenMeals, CancellationToken ct)
    {
        var incomingRecipes = chosenMeals.Values.Select(x => x.ToLower());
        
        var matchingRecipes = await ctx.Recipes
            .Where(x => incomingRecipes.Contains(x.Name.ToLower()))
            .ToListAsync(ct);

        var mappedMeals = new Dictionary<int, Recipe>();
        
        foreach (var order in chosenMeals.Keys)
        {
            var recipeName = chosenMeals[order];
            var recipe = matchingRecipes.FirstOrDefault(x => x.Name.Equals(recipeName, StringComparison.CurrentCultureIgnoreCase));
            if (recipe is null)
            {
                throw new InvalidOperationException($"Recipe for {recipeName} does not exist");
            }
            
            mappedMeals.Add(order, recipe);
        }
                
        return [.. mappedMeals.Values];
    }
}