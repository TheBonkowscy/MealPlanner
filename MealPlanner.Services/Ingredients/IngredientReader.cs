using MealPlanner.Domain.Ingredients;
using MealPlanner.Persistence;
using MealPlanner.Services.Recipes;
using MealPlanner.Shared.Ingredients;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Ingredients;

public interface IReadIngredient
{
    Task<GetIngredientsResponse> Get(CancellationToken cancellationToken);
}

public class IngredientReader(MealPlannerDbContext ctx, MeasureUnitMapper measureUnitMapper) : IReadIngredient
{
    private record IngredientProjectionDTO(int Id, string Name, IEnumerable<MeasureUnit> ApplicableUnits);
    
    public async Task<GetIngredientsResponse> Get(CancellationToken cancellationToken)
    {
        var ingredientsProjection = await ctx.Ingredients.Select(x => 
            new IngredientProjectionDTO(
                x.Id,
                x.Name,
                x.ApplicableUnits
            )).ToListAsync(cancellationToken);

        var mappedIngredients = ingredientsProjection.Select(x =>
        {
            var unitsToMap = x.ApplicableUnits.Select(measureUnitMapper.Map);
            return new IngredientListItemResponse(x.Id, x.Name, unitsToMap);
        });

        return new GetIngredientsResponse(mappedIngredients);
    }
}