using MealPlanner.Domain;
using MealPlanner.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Meals;

public interface IMapMeals
{
    Task<List<Meal>> MapMeals(Dictionary<int, string> mealsToBeMapped, CancellationToken ct);
}

public class MealMapper(MealPlannerDbContext ctx) : IMapMeals
{
    public async Task<List<Meal>> MapMeals(Dictionary<int, string> mealsToBeMapped, CancellationToken ct)
    {
        var incomingMealsNames = mealsToBeMapped.Values.Select(x => x.ToLower());
        
        var mealsFromDatabase = await ctx.Meals
            .Where(x => incomingMealsNames.Contains(x.Name.ToLower()))
            .ToListAsync(ct);

        var mappedMeals = new Dictionary<int, Meal>();
        
        foreach (var order in mealsToBeMapped.Keys)
        {
            var mealName = mealsToBeMapped[order];
            var meal = mealsFromDatabase.FirstOrDefault(x => x.Name.Equals(mealName, StringComparison.CurrentCultureIgnoreCase));
            if (meal is null)
            {
                throw new InvalidOperationException($"Meal with name {mealName} does not exist");
            }
            
            mappedMeals.Add(order, meal);
        }
                
        return [.. mappedMeals.Values];
    }
}