using MealPlanner.Domain;
using MealPlanner.Persistence;
using MealPlanner.Shared.Recipes.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Recipes;

public interface IReadRecipe
{
    Task<GetRecipesResponse> GetByQuery(string? query, CancellationToken cancellationToken = default);
    Task<GetRecipeDetailsResponse?> Get(int id, CancellationToken cancellationToken = default);
}

public class RecipeReader(MealPlannerDbContext ctx, MeasureUnitMapper measureUnitMapper) : IReadRecipe
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

        if (recipe is null) return null;
        
        var mappedIngredients = recipe.Ingredients.Select(x =>
            new UsedIngredientDetailsResponse(x.IngredientId, x.Ingredient.Name, x.Quantity, measureUnitMapper.Map(x.Unit))).ToList();
        var mappedSteps = recipe.Steps.Select(x => new StepDetailsResponse(x.Id, x.Order, x.Instructions)).ToList();
        return new GetRecipeDetailsResponse(recipe.Id, recipe.Name, 
            mappedIngredients, 
            mappedSteps);
    }
}