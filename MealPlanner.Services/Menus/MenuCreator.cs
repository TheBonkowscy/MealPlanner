using MealPlanner.Domain.Menus;
using MealPlanner.Domain.Shared;
using MealPlanner.Persistence;
using MealPlanner.Services.Shared;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Shared.Menus.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Menus;

public interface ICreateMenu
{
    Task<Result<CreateMenuResponse>> Create(CreateMenuRequest createMenuRequest, CancellationToken ct);
}

public class MenuCreator(MealPlannerDbContext ctx,
    IMapMeals mealsMapper) : ICreateMenu
{
    public async Task<Result<CreateMenuResponse>> Create(CreateMenuRequest createMenuRequest, CancellationToken ct)
    {
        var menuAlreadyExists = await ctx.Menus.AnyAsync(x => x.Date == createMenuRequest.Date, ct);
        if (menuAlreadyExists)
        {
            return Result.Failure<CreateMenuResponse>(ServiceErrors.Menu.AlreadyExists);
        }

        if (createMenuRequest.Meals is { Count: 0 })
        {
            return Result.Failure<CreateMenuResponse>(ServiceErrors.Menu.InvalidMeals);
        }
        
        var chosenRecipes = await mealsMapper.MapMeals(createMenuRequest.Meals, ct);
        if (chosenRecipes.IsFailure)
        {
            return Result.Failure<CreateMenuResponse>(chosenRecipes.Errors);
        }
        
        var result = Menu.Create(createMenuRequest.Date, chosenRecipes.Value);
        if (result.IsFailure)
        {
            return Result.Failure<CreateMenuResponse>(result.Errors);
        }

        await ctx.Menus.AddAsync(result.Value, ct);
        await ctx.SaveChangesAsync(ct);

        return Result.Success(new CreateMenuResponse(result.Value.Date));
    }
}