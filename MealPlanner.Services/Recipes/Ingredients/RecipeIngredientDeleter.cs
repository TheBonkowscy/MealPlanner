using MealPlanner.Domain.Shared;
using MealPlanner.Persistence;
using MealPlanner.Services.Shared;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Recipes.Ingredients;

public interface IDeleteRecipeIngredient
{
    Task<Result> DeleteIngredient(int recipeId,
        int ingredientId,
        string measureUnit,
        CancellationToken cancellationToken);
}

public class RecipeIngredientDeleter(MealPlannerDbContext ctx,
    MeasureUnitMapper measureUnitMapper) : IDeleteRecipeIngredient
{
    public async Task<Result> DeleteIngredient(int recipeId,
        int ingredientId,
        string measureUnit,
        CancellationToken cancellationToken)
    {
        var recipe = await ctx.Recipes.Include(x => x.Ingredients)
            .ThenInclude(x => x.Ingredient)
            .FirstOrDefaultAsync(x => x.Id == recipeId, cancellationToken: cancellationToken);

        if (recipe is null)
        {
            return Result.Failure(ServiceErrors.Recipe.DoesNotExist);
        }
        
        var ingredient = recipe.GetIngredient(ingredientId, measureUnitMapper.Map(measureUnit));
        if (ingredient is null)
        {
            return Result.Failure(ServiceErrors.Recipe.InvalidIngredients);
        }

        recipe.RemoveIngredient(ingredient);
        await ctx.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}