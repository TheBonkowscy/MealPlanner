using MealPlanner.Domain;
using MealPlanner.Persistence;
using MealPlanner.Shared.Recipes.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Recipes;

public interface IReadRecipe
{
    Task<GetRecipesResponse> GetByQuery(string? query, CancellationToken cancellationToken = default);
}
public class RecipeReader(MealPlannerDbContext ctx) : IReadRecipe
{
    public async Task<GetRecipesResponse> GetByQuery(string? query, CancellationToken cancellationToken = default)
    {
        IQueryable<Recipe> dbQuery = ctx.Recipes;
        if (!string.IsNullOrWhiteSpace(query))
        {
            dbQuery = dbQuery.Where(x => x.Name.ToLower().Contains(query.ToLower()));
        }

        var result = await dbQuery.ToListAsync(cancellationToken);
        return new GetRecipesResponse(result.Select(x => new RecipeListItemResponse(x.Id, x.Name)));
    }
}