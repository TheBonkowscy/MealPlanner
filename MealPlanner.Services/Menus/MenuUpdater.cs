using MealPlanner.Persistence;
using MealPlanner.Services.Meals;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Shared.Menus.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Menus;

public interface IUpdateMenu
{
    Task<UpdateMenuResponse> Update(UpdateMenuRequest request, CancellationToken cancellationToken);
}

public class MenuUpdater(MealPlannerDbContext ctx,
    IMapMeals mealsMapper) : IUpdateMenu
{
    public async Task<UpdateMenuResponse> Update(UpdateMenuRequest request, CancellationToken cancellationToken)
    {
        var menu = await ctx.Menus
            .Include(x => x.Items)
            .ThenInclude(x => x.Meal)
            .FirstOrDefaultAsync(x => x.Date == request.Date, cancellationToken);
        if (menu is null)
        {
            throw new InvalidOperationException($"Menu for {request.Date} does not exist.");
        }

        // 1. Remove all meals - this will work for now, revisit this when the meal model is extended
        
        ctx.MenuItems.RemoveRange(menu.Items);
        menu.RemoveAllItems();
        
        // 2. Add new meals
        if (request.Meals is { Count: 0 })
        {
            throw new InvalidOperationException("No meals were provided.");
        }
        
        var mappedMeals = await mealsMapper.MapMeals(request.Meals, cancellationToken);
        menu.AddMeals(mappedMeals);
        
        ctx.Entry(menu).State = EntityState.Modified; // TODO: Not sure if this is needed
        await ctx.SaveChangesAsync(cancellationToken);

        return new UpdateMenuResponse(menu.Date);
    }
}