using MealPlanner.Domain;

namespace MealPlanner.Services.DailyMenus.Read;

public interface IReadDailyMenu
{
    Task<Response?> Get(Guid id);
    Task<Response?> Get(DateOnly date);
}

public class DailyMenuReader(InMemoryDatabase ctx) : IReadDailyMenu
{
    public Task<Response?> Get(Guid id)
    {
        if (!ctx.Database.TryGetValue(id, out var dailyMenu))
        {
            return Task.FromResult<Response?>(null);
        }

        return MapDailyMenu(dailyMenu);
    }

    private static Task<Response?> MapDailyMenu(DailyMenu dailyMenu)
    {
        var mappedMeals = dailyMenu.Meals.Select(x => x.Value.Name);
        var mappedResponse = new Response(dailyMenu.Id, dailyMenu.Date, mappedMeals);
        return Task.FromResult<Response?>(mappedResponse);
    }

    public Task<Response?> Get(DateOnly date)
    {
        var menuForDate = ctx.Database.Values.FirstOrDefault(x => x.Date == date);
        
        if (menuForDate is null)
        {
            return Task.FromResult<Response?>(null);
        }

        return MapDailyMenu(menuForDate);
    }
}