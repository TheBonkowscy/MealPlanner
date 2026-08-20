using MealPlanner.Persistence;
using MealPlanner.Services.Recipes.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Recipes.Ingredients;

public interface IDeleteRecipeIngredient
{
    Task DeleteIngredient(int recipeId,
        int ingredientId,
        string measureUnit,
        CancellationToken cancellationToken);
}

public class RecipeIngredientDeleter(MealPlannerDbContext ctx,
    MeasureUnitMapper measureUnitMapper) : IDeleteRecipeIngredient
{
    public async Task DeleteIngredient(int recipeId,
        int ingredientId,
        string measureUnit,
        CancellationToken cancellationToken)
    {
        var recipe = await ctx.Recipes.Include(x => x.Ingredients)
            .ThenInclude(x => x.Ingredient)
            .FirstOrDefaultAsync(x => x.Id == recipeId, cancellationToken: cancellationToken);

        if (recipe is null)
        {
            throw new RecipeDoesNotExistException();
        }
        
        var ingredient = recipe.GetIngredient(ingredientId, measureUnitMapper.Map(measureUnit));
        if (ingredient is null)
        {
            throw new IngredientDoesNotExistException();
        }

        recipe.RemoveIngredient(ingredient);
        await ctx.SaveChangesAsync(cancellationToken);
    }
}