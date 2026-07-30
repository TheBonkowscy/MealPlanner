using MealPlanner.Domain;
using MealPlanner.Persistence;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Shared.Menus.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Menus.Create;

public interface ICreateMenu
{
    Task<CreateMenuResponse> Create(CreateMenuRequest createMenuRequest, CancellationToken ct);
}

public class MenuCreator(MealPlannerDbContext ctx) : ICreateMenu
{
    public async Task<CreateMenuResponse> Create(CreateMenuRequest createMenuRequest, CancellationToken ct)
    {
        try
        {
            var menuAlreadyExists = await ctx.Menus.AnyAsync(x => x.Date == createMenuRequest.Date, ct);
            if (menuAlreadyExists)
            {
                throw new InvalidOperationException($"There is already a Menu defined for {createMenuRequest.Date}.");
            }
            
            var result = Menu.Create(createMenuRequest.Date);
            if (createMenuRequest.Meals is not null)
            {
                var mappedMeals = createMenuRequest.Meals.Select(Meal.Create).ToList();
                mappedMeals.ForEach(result.AddMeal);
            }

            await ctx.Menus.AddAsync(result, ct);
            await ctx.SaveChangesAsync(ct);

            return new CreateMenuResponse(result.Id);
        }
        catch (Exception exception)
        {
            return await Task.FromException<CreateMenuResponse>(exception);
        }
    }
}