using MealPlanner.Persistence;
using MealPlanner.Services.Recipes;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Shared.Menus.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Menus;

public interface IUpdateMenu
{
    Task<UpdateMenuResponse> Update(UpdateMenuRequest request, CancellationToken cancellationToken);
}

public class MenuUpdater(MealPlannerDbContext ctx,
    IMapRecipe recipeMapper) : IUpdateMenu
{
    public async Task<UpdateMenuResponse> Update(UpdateMenuRequest request, CancellationToken cancellationToken)
    {
        var menu = await ctx.Menus
            .Include(x => x.Meals)
            .ThenInclude(x => x.Recipe)
            .FirstOrDefaultAsync(x => x.Date == request.Date, cancellationToken);
        if (menu is null)
        {
            throw new InvalidOperationException($"Menu for {request.Date} does not exist.");
        }

        // 1. Remove all meals - this will work for now, revisit this when the meal model is extended
        menu.RemoveAllItems();
        
        // 2. Add new meals
        if (request.Meals is { Count: 0 })
        {
            throw new InvalidOperationException("No meals were provided.");
        }
        
        var mappedMeals = await recipeMapper.MapRecipes(request.Meals, cancellationToken);
        menu.AddRecipes(mappedMeals);
        
        await ctx.SaveChangesAsync(cancellationToken);

        return new UpdateMenuResponse(menu.Date);
    }
}