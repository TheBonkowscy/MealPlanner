using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Menus;
using MealPlanner.Domain.Shared;
using MealPlanner.Domain.Shopping;
using MealPlanner.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Shopping.Adapters;

public class MealPlanningNeedsAdapter(MealPlannerDbContext ctx) : IShoppingNeedsSource
{
    public async Task<List<ShoppingNeed>> GetShoppingNeeds(DateOnly from, DateOnly to, CancellationToken ct)
    {
        var results = await ctx.Menus.Where(x => from <= x.Date && x.Date <= to)
            .Include(x => x.Meals)
            .ThenInclude(x => x.Recipe)
            .ThenInclude(x => x.Ingredients)
            .ThenInclude(x => x.Ingredient)
            .ToListAsync(ct);
        
        return MapToNeeds(results);
    }

    private static List<ShoppingNeed> MapToNeeds(List<Menu> menus)
    {
        var allNeedsForThePeriod = new List<ShoppingNeed>();

        foreach (var menu in menus)
        {
            foreach (var meal in menu.Meals)
            {
                var usedIngredients = meal.Recipe.Ingredients.ToList();
                var mappedIngredients = usedIngredients.Select(x => new ShoppingNeed(
                    x.IngredientId,
                    x.Ingredient.Name,
                    meal.ScaledQuantityOf(x),
                    x.Unit,
                    x.Ingredient.ShelfLifeDays,
                    menu.Date
                ));
                allNeedsForThePeriod.AddRange(mappedIngredients);
            }
        }

        return allNeedsForThePeriod;
    }
}