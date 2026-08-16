using MealPlanner.Domain.Menus.Actions;
using MealPlanner.Persistence;
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
        var incomingRecipesIds = chosenMeals.Select(x => x.Id);
        
        var matchingRecipes = await ctx.Recipes
            .Where(x => incomingRecipesIds.Contains(x.Id))
            .ToListAsync(ct);
        
        if (matchingRecipes.Count != incomingRecipesIds.Count())
        {
            throw new InvalidOperationException("One or more recipes was not found");   // TODO: custom exception?
        }

        var recipesById = matchingRecipes
            .ToDictionary(x => x, x => chosenMeals.FirstOrDefault(y => y.Id == x.Id));

        var mappedMeals = new List<AddMealAction>();
        
        foreach (var recipe in recipesById.Keys)
        {
            var request = recipesById[recipe];
            if (request is null)
            {
                throw new InvalidOperationException($"Could not find details of recipe `{recipe.Name}`. Make sure your request is properly formatted and try again.");
            }
            
            mappedMeals.Add(AddMealAction.Create(recipe, request.Order, request.Servings));
        }

        return mappedMeals;
    }
}