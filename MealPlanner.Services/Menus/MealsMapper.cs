using MealPlanner.Domain.Menus.Actions;
using MealPlanner.Persistence;
using MealPlanner.Services.Menus.Exceptions;
using MealPlanner.Shared.Menus.Requests;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Menus;

public interface IMapMeals
{
    Task<List<AddMealAction>> MapMeals(List<AddMealRequest> chosenMeals, CancellationToken ct);
}

public class MealsMapper(MealPlannerDbContext ctx) : IMapMeals
{
    public async Task<List<AddMealAction>> MapMeals(List<AddMealRequest> chosenMeals, CancellationToken ct)
    {
        var incomingRecipesIds = chosenMeals.Select(x => x.Id).ToList();
        
        var matchingRecipes = await ctx.Recipes
            .Where(x => incomingRecipesIds.Contains(x.Id))
            .ToListAsync(ct);
        
        if (matchingRecipes.Count != incomingRecipesIds.Count)
        {
            var missingIds = incomingRecipesIds.Except(matchingRecipes.Select(x => x.Id));
            throw new MissingRecipesException(missingIds);
        }

        var recipesById = matchingRecipes
            .ToDictionary(x => x, x => chosenMeals.FirstOrDefault(y => y.Id == x.Id));

        var mappedMeals = new List<AddMealAction>();
        
        foreach (var recipe in recipesById.Keys)
        {
            var request = recipesById[recipe];
            if (request is null)
            {
                throw new RecipeDetailsMissingException(recipe.Name);
            }
            
            mappedMeals.Add(AddMealAction.Create(recipe, request.Order, request.Servings));
        }

        return mappedMeals;
    }
}