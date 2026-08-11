using MealPlanner.Persistence;
using MealPlanner.Shared.Ingredients;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Ingredients;

public interface IReadIngredient
{
    Task<GetIngredientsResponse> Get(CancellationToken cancellationToken);
}
public class IngredientReader(MealPlannerDbContext ctx) : IReadIngredient
{
    public async Task<GetIngredientsResponse> Get(CancellationToken cancellationToken)
    {
        var result = await ctx.Ingredients.Select(x => 
            new IngredientListItemResponse(
                x.Id,
                x.Name,
                x.ApplicableUnits.Select(z => new IngredientMeasureUnitsResponse(
                    z.ToString(),
                    z.ToString()))
            )).ToListAsync(cancellationToken);

        return new GetIngredientsResponse(result);
    }
}