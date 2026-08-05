using MealPlanner.Domain;
using MealPlanner.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Meals;

public interface IMapMeals
{
    Task<List<Meal>> MapMeals(IEnumerable<string> mealsToBeMapped, CancellationToken ct);
}

public class MealMapper(MealPlannerDbContext ctx) : IMapMeals
{
    public async Task<List<Meal>> MapMeals(IEnumerable<string> mealsToBeMapped, CancellationToken ct)
    {
        List<Meal> mappedMeals = [];
        
        var mealsRequestedToBeAdded = mealsToBeMapped.Select(x => x.ToLower());
        if (mealsRequestedToBeAdded is null) return mappedMeals;
        
        var mealsThatAlreadyExist = await ctx.Meals
            .Where(x => mealsRequestedToBeAdded.Contains(x.Name.ToLower()))
            .ToListAsync(ct);
        // TODO: keep order of everything
        mealsThatAlreadyExist.ForEach(mappedMeals.Add);
                
        var namesOfMealsThatAlreadyExist = mealsThatAlreadyExist.Select(x => x.Name.ToLower());
        var mealsToCreate = mealsRequestedToBeAdded
            .Except(namesOfMealsThatAlreadyExist)
            .Select(Meal.Create).ToList();
        
        // TODO: keep order of everything
        mealsToCreate.ForEach(mappedMeals.Add);
        
        return mappedMeals;
    }
}