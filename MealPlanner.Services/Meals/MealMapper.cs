using MealPlanner.Domain;
using MealPlanner.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Meals;

public interface IMapMeals
{
    Task<List<Recipe>> MapMeals(Dictionary<int, string> mealsToBeMapped, CancellationToken ct);
}

public class MealMapper(MealPlannerDbContext ctx) : IMapMeals
{
    public async Task<List<Recipe>> MapMeals(Dictionary<int, string> mealsToBeMapped, CancellationToken ct)
    {
        var incomingRecipes = mealsToBeMapped.Values.Select(x => x.ToLower());
        
        var recipesFromDatabase = await ctx.Recipes
            .Where(x => incomingRecipes.Contains(x.Name.ToLower()))
            .ToListAsync(ct);

        var mappedMeals = new Dictionary<int, Recipe>();
        
        foreach (var order in mealsToBeMapped.Keys)
        {
            var recipeName = mealsToBeMapped[order];
            var meal = recipesFromDatabase.FirstOrDefault(x => x.Name.Equals(recipeName, StringComparison.CurrentCultureIgnoreCase));
            if (meal is null)
            {
                throw new InvalidOperationException($"Recipe {recipeName} does not exist");
            }
            
            mappedMeals.Add(order, meal);
        }
                
        return [.. mappedMeals.Values];
    }
}