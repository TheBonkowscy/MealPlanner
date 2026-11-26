using System.Globalization;
using MealPlanner.Domain.Menus;
using MealPlanner.Persistence;
using MealPlanner.Shared.Menus.Responses;
using MealPlanner.Shared.Recipes.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Menus;

public interface IReadMenu
{
    Task<GetMenuResponse?> Get(DateOnly date, CancellationToken ct);
    Task<GetExistingMenusResponse> GetRange(DateOnly? from, DateOnly? to, CancellationToken ct);
    Task<GetWeekResponse> GetWeek(DateTime day, CancellationToken cancellationToken);
}

public class MenuReader(MealPlannerDbContext ctx) : IReadMenu
{
    private static GetMenuResponse MapMenu(Menu menu)
    {
        var mappedMeals = menu.Meals
            .Select(x => new OrderedRecipeListItemResponse(x.Recipe.Id, x.Order, x.Servings, x.Recipe.Name))
            .OrderBy(x => x.Order)
            .ToList();
        return new GetMenuResponse(menu.Id, menu.Date, mappedMeals);
    }

    public async Task<GetMenuResponse?> Get(DateOnly date, CancellationToken ct)
    {
        var menuForDate = await ctx.Menus
            .Include(x => x.Meals)
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

    public async Task<GetWeekResponse> GetWeek(DateTime day, CancellationToken cancellationToken)
    {
        var allDaysInThisWeek = Enum.GetValues<DayOfWeek>()
            .Select(x => DateOnly.FromDateTime(
                ISOWeek.ToDateTime(
                    ISOWeek.GetYear(day),
                    ISOWeek.GetWeekOfYear(day),
                    x)))
            .ToList();
        
        var menuForTheWeek = await ctx.Menus.Where(x => allDaysInThisWeek.Contains(x.Date))
            .Include(x => x.Meals)
            .ThenInclude(x => x.Recipe)
            .Select(x => MapMenu(x))
            .ToListAsync(cancellationToken);
        
        FillInMissingMenus(allDaysInThisWeek, menuForTheWeek);

        return new GetWeekResponse(
            [.. menuForTheWeek.OrderBy(x => x.Date)],
            allDaysInThisWeek.First(x => x.DayOfWeek == DayOfWeek.Monday));
    }

    private static void FillInMissingMenus(List<DateOnly> allDaysInThisWeek, List<GetMenuResponse> menuForTheWeek)
    {
        foreach (var date in allDaysInThisWeek)
        {
            var menuForDay = menuForTheWeek.FirstOrDefault(x => x.Date == date);
            if (menuForDay is null)
            {
                menuForTheWeek.Add(GetMenuResponse.Empty(date));
            }
        }
    }
}