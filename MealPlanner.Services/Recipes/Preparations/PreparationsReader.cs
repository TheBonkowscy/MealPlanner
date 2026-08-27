using MealPlanner.Persistence;
using MealPlanner.Shared.Recipes.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Recipes.Preparations;

public interface IReadPreparations
{
    Task<GetRecipePreparationsDetailsResponse> Get(DateOnly date, CancellationToken cancellationToken);
}

public class PreparationsReader(MealPlannerDbContext ctx) : IReadPreparations
{
    public async Task<GetRecipePreparationsDetailsResponse> Get(DateOnly date, CancellationToken cancellationToken)
    {
        var activeMeals = await ctx.Menus
            .AsNoTracking()
            .Where(menu => menu.Date >= date)
            .SelectMany(menu => menu.Meals.Select(meal => new 
            { 
                meal.RecipeId, 
                TargetMealDate = menu.Date 
            }))
            .ToListAsync(cancellationToken);

        if (activeMeals.Count == 0)
        {
            return GetRecipePreparationsDetailsResponse.Empty;
        }

        var recipeIds = activeMeals.Select(x => x.RecipeId).Distinct().ToList();
        var recipesWithPreps = await ctx.Recipes
            .AsNoTracking()
            .Where(r => recipeIds.Contains(r.Id))
            .Select(r => new
            {
                r.Id,
                r.Name,
                r.Preparations
            })
            .ToListAsync(cancellationToken);

        var recipesDict = recipesWithPreps.ToDictionary(r => r.Id);

        var result = activeMeals
            .Where(meal => recipesDict.ContainsKey(meal.RecipeId))
            .SelectMany(meal => recipesDict[meal.RecipeId].Preparations
                .Where(prep => meal.TargetMealDate.AddDays(-prep.LeadDays) == date)
                .Select(prep => new RecipePreparationsDescriptionResponse(
                    meal.RecipeId,
                    recipesDict[meal.RecipeId].Name,
                    meal.TargetMealDate,
                    prep.LeadDays,
                    prep.Required,
                    prep.Description
                ))
            )
            .ToList();

        return new GetRecipePreparationsDetailsResponse(result);
    }
}