using MealPlanner.Domain;
using MealPlanner.Persistence;
using MealPlanner.Shared.Menus.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Menus;

public interface IReadMenu
{
    Task<GetMenuResponse?> Get(int id, CancellationToken ct);
    Task<GetMenuResponse?> Get(DateOnly date, CancellationToken ct);
    Task<GetExistingMenusResponse> GetRange(DateOnly? from, DateOnly? to, CancellationToken ct);
}

public class MenuReader(MealPlannerDbContext ctx) : IReadMenu
{
    public async Task<GetMenuResponse?> Get(int id, CancellationToken ct)
    {
        var menu = await ctx.Menus
            .Include(x => x.Items)
            .ThenInclude(x => x.Recipe)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return menu is null ? null : MapMenu(menu);
    }

    private static GetMenuResponse MapMenu(Menu menu)
    {
        var mappedMeals = menu.Items.ToDictionary(x => x.Order, x => x.Recipe.Name);
        return new GetMenuResponse(menu.Id, menu.Date, mappedMeals);
    }

    public async Task<GetMenuResponse?> Get(DateOnly date, CancellationToken ct)
    {
        var menuForDate = await ctx.Menus
            .Include(x => x.Items)
            .ThenInclude(x => x.Recipe)
            .FirstOrDefaultAsync(x => x.Date == date, ct);
        
        return menuForDate is null ? null : MapMenu(menuForDate);
    }

    public async Task<GetExistingMenusResponse> GetRange(DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        IQueryable<Menu> query = ctx.Menus;

        if (from is not null)
        {
            query = query.Where(x => x.Date >= from);
        }

        if (to is not null)
        {
            query = query.Where(x => x.Date <= to);
        }
        
        var result =  await query.ToListAsync(ct);

        var mappedResult = result.Select(x => new ExistingMenuListItem(x.Id, x.Date));
        
        return new GetExistingMenusResponse(mappedResult);
    }
}