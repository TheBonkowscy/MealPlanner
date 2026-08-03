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
            var mealsToAdd = createMenuRequest.Meals?.Select(x => x.ToLower());
            if (mealsToAdd is not null)
            {
                var mealsThatAlreadyExist = await ctx.Meals
                    .Where(x => mealsToAdd.Contains(x.Name.ToLower()))
                    .ToListAsync(ct);
                mealsThatAlreadyExist.ForEach(result.AddMeal);
                
                var namesOfMealsThatAlreadyExist = mealsThatAlreadyExist.Select(x => x.Name.ToLower());
                var mealsToCreate = mealsToAdd
                    .Except(namesOfMealsThatAlreadyExist)
                    .Select(Meal.Create).ToList();
                
                mealsToCreate.ForEach(result.AddMeal);
                
            }

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