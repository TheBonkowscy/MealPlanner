using MealPlanner.Domain;
using MealPlanner.Domain.Menus;
using MealPlanner.Persistence;
using MealPlanner.Services.Menus.Exceptions;
using MealPlanner.Services.Recipes;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Shared.Menus.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Menus;

public interface ICreateMenu
{
    Task<CreateMenuResponse> Create(CreateMenuRequest createMenuRequest, CancellationToken ct);
}

public class MenuCreator(MealPlannerDbContext ctx,
    IMapMeals mealsMapper) : ICreateMenu
{
    public async Task<CreateMenuResponse> Create(CreateMenuRequest createMenuRequest, CancellationToken ct)
    {
        try
        {
            var menuAlreadyExists = await ctx.Menus.AnyAsync(x => x.Date == createMenuRequest.Date, ct);
            if (menuAlreadyExists)
            {
                throw new MenuAlreadyExistsException(createMenuRequest.Date);
            }

            if (createMenuRequest.Meals is { Count: 0 })
            {
                throw new MissingMealsException();
            }
            
            var chosenRecipes = await mealsMapper.MapMeals(createMenuRequest.Meals, ct);
            
            var result = Menu.Create(createMenuRequest.Date, chosenRecipes);

            await ctx.Menus.AddAsync(result, ct);
            await ctx.SaveChangesAsync(ct);

            return new CreateMenuResponse(result.Date);
        }
        catch (Exception exception)
        {
            return await Task.FromException<CreateMenuResponse>(exception);
        }
    }
}