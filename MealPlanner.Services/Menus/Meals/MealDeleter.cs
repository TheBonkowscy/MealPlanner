using MealPlanner.Domain.Shared;
using MealPlanner.Persistence;
using MealPlanner.Services.Shared;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Menus.Meals;

public interface IDeleteMeals
{
    Task<Result> Delete(DateOnly day, int mealId, CancellationToken token);
}

public class MealDeleter(MealPlannerDbContext ctx) : IDeleteMeals
{
    public async Task<Result> Delete(DateOnly day, int mealId, CancellationToken token)
    {
        var existingMenu = await ctx.Menus
            .Include(x => x.Meals).FirstOrDefaultAsync(x => x.Date == day, token);
        if (existingMenu is null)
        {
            return Result.Failure(ServiceErrors.Menu.DoesNotExist(day));
        }
        
        var removed = existingMenu.RemoveMeal(mealId);
        if (removed.IsFailure)
        {
            return removed;
        }

        if (!existingMenu.Meals.Any())
        {
            ctx.Menus.Remove(existingMenu);
        }
        
        await ctx.SaveChangesAsync(token);
        
        return Result.Success();
    }
}