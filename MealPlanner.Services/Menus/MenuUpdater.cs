using MealPlanner.Domain.Shared;
using MealPlanner.Persistence;
using MealPlanner.Services.Shared;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Shared.Menus.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Menus;

public interface IUpdateMenu
{
    Task<Result<UpdateMenuResponse>> Update(UpdateMenuRequest request, CancellationToken cancellationToken);
}

public class MenuUpdater(MealPlannerDbContext ctx,
    IMapMeals mealsMapper) : IUpdateMenu
{
    public async Task<Result<UpdateMenuResponse>> Update(UpdateMenuRequest request, CancellationToken cancellationToken)
    {
        if (request.Meals is { Count: 0 })
        {
            return Result.Failure<UpdateMenuResponse>(ServiceErrors.Menu.InvalidMeals);
        }
        
        var menu = await ctx.Menus
            .Include(x => x.Meals)
            .ThenInclude(x => x.Recipe)
            .FirstOrDefaultAsync(x => x.Date == request.Date, cancellationToken);
        if (menu is null)
        {
            return Result.Failure<UpdateMenuResponse>(ServiceErrors.Menu.DoesNotExist(request.Date));
        }

        // 1. Remove all meals - this will work for now, revisit this when the meal model is extended
        // TODO: revising updating meals in place. Maybe extending this to support PUT/DELETE meal from Menu would be a good idea?
        menu.RemoveAllItems();
        
        // 2. Add new meals
        var mappedMeals = await mealsMapper.MapMeals(request.Meals, cancellationToken);
        if (mappedMeals.IsFailure)
        {
            return Result.Failure<UpdateMenuResponse>(mappedMeals.Error);
        }

        var errorsOnAdd = mappedMeals.Value.OrderBy(x => x.Order)
            .Select(menu.AddMeal).AllErrors();
        if (errorsOnAdd.Count != 0)
        {
            return Result.Failure<UpdateMenuResponse>(errorsOnAdd);
        }
        
        await ctx.SaveChangesAsync(cancellationToken);
        return Result.Success(new UpdateMenuResponse(menu.Date));
    }
}