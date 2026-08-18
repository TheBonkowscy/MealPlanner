using MealPlanner.Domain;
using MealPlanner.Persistence;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Recipes.Steps;

public interface ICreateRecipeStep
{
    Task<GetRecipeDetailsResponse> CreateStep(int id, CreateRecipeStepRequest request, CancellationToken cancellationToken);
}

public class RecipeStepCreator(MealPlannerDbContext ctx, RecipeMapper recipeMapper) : ICreateRecipeStep
{
    public async Task<GetRecipeDetailsResponse> CreateStep(int id, CreateRecipeStepRequest request, CancellationToken cancellationToken)
    {
        var recipe = await ctx.Recipes
            .Include(recipe => recipe.Steps)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (recipe is null)
        {
            throw new InvalidOperationException("Recipe could not be found");   // TODO: custom exceptions?
        }
        
        recipe.AddStep(request.Order, request.Instructions);
        await ctx.SaveChangesAsync(cancellationToken);

        return recipeMapper.ToDetails(recipe);
    }
}