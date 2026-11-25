using MealPlanner.Domain.Menus;
using MealPlanner.Domain.Menus.Actions;
using MealPlanner.Domain.Shared;
using MealPlanner.Persistence;
using MealPlanner.Services.Shared;
using MealPlanner.Shared.Menus.Meals;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Menus.Meals;

public interface ITransferMeals
{
    Task<Result> Transfer(DateOnly date, int mealId, TransferMealRequest request, CancellationToken token);
}

public class MealTransferer(MealPlannerDbContext ctx) : ITransferMeals
{
    public async Task<Result> Transfer(DateOnly date, int mealId, TransferMealRequest request, CancellationToken token)
    {
        var targetDate = request.NewDate;
        if (date == targetDate)
        {
            return Result.Failure(ServiceErrors.Meal.TransferToTheSameDay);
        }
        
        DateOnly[] requiredMenus = [date, targetDate];
        var menus = await ctx.Menus
            .Include(x => x.Meals).ThenInclude(meal => meal.Recipe)
            .Where(x => requiredMenus.Contains(x.Date)).ToListAsync(token);
        
        var sourceMenu = menus.FirstOrDefault(x => x.Date == date);
        if (sourceMenu == null)
        {
            return Result.Failure(ServiceErrors.Menu.DoesNotExist(date));
        }
        
        var meal = sourceMenu.Meals.FirstOrDefault(x => x.RecipeId == mealId);
        if (meal is null)
        {
            return Result.Failure(DomainErrors.Meal.NotFoundInTheDay(mealId));
        }
        
        var (targetMenu, result, isNewMenu) = await GetOrCreateMenu(menus, targetDate);
        if (result.IsFailure)
        {
            return result;
        }

        var addMealAction = AddMealAction.Create(meal.Recipe, request.Order, meal.Servings);
        if (addMealAction.IsFailure)
        {
            return addMealAction;
        }

        var deleteSourceMenu = false;
        if (request.TransferMode is TransferMode.Move)
        {
            var removed = sourceMenu.RemoveMeal(meal.Recipe.Id);
            if (removed.IsFailure)
            {
                return removed;
            }

            if (sourceMenu.IsEmpty())
            {
                deleteSourceMenu = true;
            }
        }

        var inserted = targetMenu!.InsertMeal(addMealAction.Value);
        if (inserted.IsFailure)
        {
            return inserted;
        }

        if (isNewMenu)
        {
            await ctx.AddAsync(targetMenu, token);
        }

        if (deleteSourceMenu)
        {
            ctx.Menus.Remove(sourceMenu);
        }
        
        await ctx.SaveChangesAsync(token);
        return Result.Success();
    }

    private async Task<(Menu? targetMenu, Result result, bool isNewMenu)> GetOrCreateMenu(List<Menu> menus, DateOnly targetDate)
    {
        var targetMenu = menus.FirstOrDefault(x => x.Date == targetDate);
        if (targetMenu is not null) return (targetMenu, Result.Success(), false);
        var newTargetMenu = Menu.Create(targetDate, []);
        if (newTargetMenu.IsFailure)
        {
            return (targetMenu, newTargetMenu, false);
        }
            
        targetMenu = newTargetMenu.Value;
        return (targetMenu, newTargetMenu, true);
    }
}
