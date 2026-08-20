using MealPlanner.Domain;
using MealPlanner.Domain.Recipes;
using MealPlanner.Persistence;
using MealPlanner.Shared.Recipes.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Recipes;

public interface IReadRecipe
{
    Task<GetRecipesResponse> GetByQuery(string? query, CancellationToken cancellationToken = default);
    Task<GetRecipeDetailsResponse?> Get(int id, CancellationToken cancellationToken = default);
}

public class RecipeReader(MealPlannerDbContext ctx, RecipeMapper recipeMapper) : IReadRecipe
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

    public async Task<GetRecipeDetailsResponse?> Get(int id, CancellationToken cancellationToken = default)
    {
        var recipe = await ctx.Recipes
            .Include(x => x.Ingredients).ThenInclude(x => x.Ingredient)
            .Include(x => x.Steps).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return recipe is null ? null : recipeMapper.ToDetails(recipe);
    }
}