using MealPlanner.Domain.Ingredients.Actions;
using MealPlanner.Persistence;
using MealPlanner.Services.Recipes.Exceptions;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Recipes.Ingredients;

public interface IUpdateRecipeIngredient
{
    Task<GetRecipeDetailsResponse> UpdateIngredient(int recipeId,
        int ingredientId,
        UpdateRecipeIngredientRequest request,
        CancellationToken cancellationToken);
}

public class RecipeIngredientUpdater(MealPlannerDbContext ctx,
    MeasureUnitMapper measureUnitMapper,
    RecipeMapper recipeMapper) : IUpdateRecipeIngredient
{
    public async Task<GetRecipeDetailsResponse> UpdateIngredient(int recipeId,
        int ingredientId,
        UpdateRecipeIngredientRequest request,
        CancellationToken cancellationToken)
    {
        var recipe = await ctx.Recipes.Include(x => x.Ingredients)
            .ThenInclude(x => x.Ingredient)
            .FirstOrDefaultAsync(x => x.Id == recipeId, cancellationToken: cancellationToken);

        if (recipe is null)
        {
            throw new RecipeDoesNotExistException();
        }

        var measureUnit = measureUnitMapper.Map(request.Unit);
        var usedIngredient = recipe.GetIngredient(ingredientId, measureUnit);
        if (usedIngredient is not null)
        {
            usedIngredient.UpdateQuantity(request.Quantity);
        }
        else
        {
            var ingredient = await ctx.Ingredients.FirstOrDefaultAsync(x => x.Id == ingredientId, cancellationToken);
            if (ingredient is null)
            {
                throw new IngredientDoesNotExistException();
            }
            var addIngredient = AddIngredientAction.Create(ingredient, request.Quantity, measureUnit);
            recipe.AddIngredient(addIngredient);
        }
        
        await ctx.SaveChangesAsync(cancellationToken);

        return recipeMapper.ToDetails(recipe);
    }
}