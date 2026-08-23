using MealPlanner.Domain.Menus.Actions;
using MealPlanner.Domain.Shared;
using MealPlanner.Persistence;
using MealPlanner.Shared.Menus.Requests;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Menus;

public interface IMapMeals
{
    Task<Result<List<AddMealAction>>> MapMeals(List<AddMealRequest> chosenMeals, CancellationToken ct);
}

public class MealsMapper(MealPlannerDbContext ctx) : IMapMeals
{
    public async Task<Result<List<AddMealAction>>> MapMeals(List<AddMealRequest> chosenMeals, CancellationToken ct)
    {
        var incomingRecipesIds = chosenMeals.Select(x => x.Id).ToList();
        
        var matchingRecipes = await ctx.Recipes
            .Where(x => incomingRecipesIds.Contains(x.Id))
            .ToListAsync(ct);
        
        if (matchingRecipes.Count != incomingRecipesIds.Count)
        {
            var missingIds = incomingRecipesIds.Except(matchingRecipes.Select(x => x.Id)).ToArray();
            return Result.Failure<List<AddMealAction>>(ServiceErrors.Meal.MissingRecipesById(missingIds));
        }

        var recipesById = matchingRecipes
            .ToDictionary(x => x, x => chosenMeals.FirstOrDefault(y => y.Id == x.Id));

        var mappedMeals = new List<AddMealAction>();
        var errors = new List<Error>();
        
        foreach (var recipe in recipesById.Keys)
        {
            var request = recipesById[recipe];
            if (request is null)
            {
                return Result.Failure<List<AddMealAction>>(ServiceErrors.Meal.DetailsMissing);
            }

            var result = AddMealAction.Create(recipe, request.Order, request.Servings);
            if (result.IsFailure)
            {
                errors.AddRange(result.Errors);
            }
            else
            {
                mappedMeals.Add(result.Value);
            }
        }

        return errors.Count != 0
            ? Result.Failure<List<AddMealAction>>(errors)
            : Result.Success(mappedMeals);
    }
}