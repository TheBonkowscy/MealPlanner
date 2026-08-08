using MealPlanner.Domain;
using MealPlanner.Persistence;
using MealPlanner.Shared.Meals;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Meals.Read;

public interface IReadRecipes
{
    Task<GetRecipesResponse> GetByQuery(string? query, CancellationToken cancellationToken = default);
}
public class RecipesReader(MealPlannerDbContext ctx) : IReadRecipes
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