using MealPlanner.Domain;
using MealPlanner.Persistence;
using MealPlanner.Shared.Menus.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Menus.Read;

public interface IReadMenu
{
    Task<GetMenuResponse?> Get(int id);
    Task<GetMenuResponse?> Get(DateOnly date);
}

public class MenuReader(MealPlannerDbContext ctx) : IReadMenu
{
    public async Task<GetMenuResponse?> Get(int id)
    {
        var menu = await ctx.Menus
            .Include(x => x.Items)
            .ThenInclude(x => x.Meal)
            .FirstOrDefaultAsync(x => x.Id == id);
        return menu is null ? null : MapMenu(menu);
    }

    private static GetMenuResponse MapMenu(Menu menu)
    {
        var mappedMeals = menu.Items.Select(x => x.Meal.Name);
        return new GetMenuResponse(menu.Id, menu.Date, mappedMeals);
    }

    public async Task<GetMenuResponse?> Get(DateOnly date)
    {
        var menuForDate = await ctx.Menus
            .Include(x => x.Items)
            .ThenInclude(x => x.Meal)
            .FirstOrDefaultAsync(x => x.Date == date);
        
        return menuForDate is null ? null : MapMenu(menuForDate);
    }
}