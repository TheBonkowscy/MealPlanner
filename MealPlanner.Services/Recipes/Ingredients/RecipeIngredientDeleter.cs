using MealPlanner.Persistence;
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
            .ThenInclude(x => x.IngredientId)
            .FirstOrDefaultAsync(x => x.Id == recipeId, cancellationToken: cancellationToken);

        if (recipe is null)
        {
            throw new InvalidOperationException($"Recipe could not be found");   // TODO: custom exceptions?
        }
        
        var ingredient = recipe.GetIngredient(ingredientId, measureUnitMapper.Map(measureUnit));
        if (ingredient is null)
        {
            throw new InvalidOperationException($"Specified ingredient could not be found");   // TODO: custom exceptions?
        }

        recipe.RemoveIngredient(ingredient);
        await ctx.SaveChangesAsync(cancellationToken);
    }
}