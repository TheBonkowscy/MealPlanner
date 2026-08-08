using MealPlanner.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Menus;

public interface IDeleteMenu
{
    Task Delete(DateOnly date, CancellationToken cancellationToken = default);
}

public class MenuDeleter(MealPlannerDbContext ctx) : IDeleteMenu
{
    public async Task Delete(DateOnly date, CancellationToken cancellationToken)
    {
        var existingMenu = await ctx.Menus.FirstOrDefaultAsync(x => x.Date == date, cancellationToken);
        if (existingMenu is null) return;
        ctx.Menus.Remove(existingMenu);
        await ctx.SaveChangesAsync(cancellationToken);
    }
}