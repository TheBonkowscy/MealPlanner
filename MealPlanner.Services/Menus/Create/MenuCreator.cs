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
            var mappedMeals = await MapMeals(createMenuRequest, ct);
            
            var result = Menu.Create(createMenuRequest.Date, mappedMeals);

            await ctx.Menus.AddAsync(result, ct);
            await ctx.SaveChangesAsync(ct);

            return new CreateMenuResponse(result.Date);
        }
        catch (Exception exception)
        {
            return await Task.FromException<CreateMenuResponse>(exception);
        }
    }

    private async Task<List<Meal>> MapMeals(CreateMenuRequest request, CancellationToken ct)
    {
        List<Meal> mappedMeals = [];
        
        var mealsRequestedToBeAdded = request.Meals?.Select(x => x.ToLower());
        if (mealsRequestedToBeAdded is null) return mappedMeals;
        
        var mealsThatAlreadyExist = await ctx.Meals
            .Where(x => mealsRequestedToBeAdded.Contains(x.Name.ToLower()))
            .ToListAsync(ct);
        // TODO: keep order of everything
        mealsThatAlreadyExist.ForEach(mappedMeals.Add);
                
        var namesOfMealsThatAlreadyExist = mealsThatAlreadyExist.Select(x => x.Name.ToLower());
        var mealsToCreate = mealsRequestedToBeAdded
            .Except(namesOfMealsThatAlreadyExist)
            .Select(Meal.Create).ToList();
        // TODO: keep order of everything
        mealsToCreate.ForEach(mappedMeals.Add);
        
        return mappedMeals;
    }
}