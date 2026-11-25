using MealPlanner.Domain.Menus;
using MealPlanner.Domain.Menus.Actions;
using MealPlanner.Domain.Shared;
using MealPlanner.Persistence;
using MealPlanner.Services.Shared;
using MealPlanner.Shared.Menus.Requests;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Menus.Meals;

public interface ICreateMeals
{
    Task<Result> Create(DateOnly day, CreateMealRequest createMealRequest, CancellationToken token);
}

public class MealCreator(MealPlannerDbContext ctx) : ICreateMeals
{
    public async Task<Result> Create(DateOnly day, CreateMealRequest createMealRequest, CancellationToken token)
    {
        var recipe = await ctx.Recipes.FirstOrDefaultAsync(x => x.Id == createMealRequest.RecipeId, token);
        if (recipe is null)
        {
            return Result.Failure(ServiceErrors.Meal.MissingRecipesById(createMealRequest.RecipeId));
        }
        var mealToAdd = AddMealAction.Create(recipe, createMealRequest.Order, createMealRequest.Servings);
        if (mealToAdd.IsFailure)
        {
            return mealToAdd;
        }
        
        var menu = await ctx.Menus.Include(x => x.Meals).FirstOrDefaultAsync(x => x.Date == day, cancellationToken: token);
        var isNew = menu is null;
        if (isNew)
        {
            var created = Menu.Create(day, []);
            if (created.IsFailure)
            {
                return created;
            }
            
            menu = created.Value;
        }
        
        var inserted = menu!.InsertMeal(mealToAdd.Value);
        if (inserted.IsFailure)
        {
            return inserted;
        }

        if (isNew)
        {
            await ctx.Menus.AddAsync(menu, token);
        }
        else
        {
            ctx.Menus.Update(menu);
        }

        await ctx.SaveChangesAsync(token);
        return Result.Success();
    }
}