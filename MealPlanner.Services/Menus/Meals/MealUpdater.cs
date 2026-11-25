using MealPlanner.Domain.Shared;
using MealPlanner.Persistence;
using MealPlanner.Services.Shared;
using MealPlanner.Shared.Menus.Requests;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Menus.Meals;

public interface IUpdateMeals
{
    Task<Result> Update(DateOnly date, int recipeId, UpdateMealRequest request, CancellationToken token);
}

public class MealUpdater(MealPlannerDbContext ctx) : IUpdateMeals
{
    public async Task<Result> Update(DateOnly date, int recipeId, UpdateMealRequest request, CancellationToken token)
    {
        var existingMenu = await ctx.Menus
            .Include(x => x.Meals).FirstOrDefaultAsync(x => x.Date == date, token);
        if (existingMenu is null)
        {
            return Result.Failure(ServiceErrors.Menu.DoesNotExist(date));
        }

        if (request.Servings is null && request.Position is null)
        {
            return Result.Failure(ServiceErrors.Meal.NothingToUpdateForMeal);
        }

        if (request.Position is not null)
        {
            var result = existingMenu.MoveMeal(recipeId, request.Position.Value);
            if (result.IsFailure)
            {
                return result;
            }
        }

        if (request.Servings is not null)
        {
            var result = existingMenu.ChangeServings(recipeId, request.Servings.Value);
            if (result.IsFailure)
            {
                return result;
            }
        }

        ctx.Menus.Update(existingMenu);
        await ctx.SaveChangesAsync(token);
        return Result.Success();
    }
}