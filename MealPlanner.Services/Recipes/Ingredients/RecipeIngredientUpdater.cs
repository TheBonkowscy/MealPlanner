using MealPlanner.Domain.Ingredients.Actions;
using MealPlanner.Domain.Shared;
using MealPlanner.Persistence;
using MealPlanner.Services.Shared;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Recipes.Ingredients;

public interface IUpdateRecipeIngredient
{
    Task<Result<GetRecipeDetailsResponse>> UpdateIngredient(int recipeId,
        int ingredientId,
        UpdateRecipeIngredientRequest request,
        CancellationToken cancellationToken);
}

public class RecipeIngredientUpdater(MealPlannerDbContext ctx,
    MeasureUnitMapper measureUnitMapper,
    RecipeMapper recipeMapper) : IUpdateRecipeIngredient
{
    public async Task<Result<GetRecipeDetailsResponse>> UpdateIngredient(int recipeId,
        int ingredientId,
        UpdateRecipeIngredientRequest request,
        CancellationToken cancellationToken)
    {
        var recipe = await ctx.Recipes.Include(x => x.Ingredients)
            .ThenInclude(x => x.Ingredient)
            .FirstOrDefaultAsync(x => x.Id == recipeId, cancellationToken: cancellationToken);

        if (recipe is null)
        {
            return Result.Failure<GetRecipeDetailsResponse>(ServiceErrors.Recipe.DoesNotExist);
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
                return Result.Failure<GetRecipeDetailsResponse>(ServiceErrors.Recipe.InvalidIngredients);
            }
            
            var addIngredient = AddIngredientAction.Create(ingredient, request.Quantity, measureUnit);
            if (addIngredient.IsFailure)
            {
                return Result.Failure<GetRecipeDetailsResponse>(addIngredient.Errors);
            }
            
            var ingredientAdded = recipe.AddIngredient(addIngredient.Value);
            if (ingredientAdded.IsFailure)
            {
                return Result.Failure<GetRecipeDetailsResponse>(ingredientAdded.Errors);
            }
        }
        
        await ctx.SaveChangesAsync(cancellationToken);

        return Result.Success(recipeMapper.ToDetails(recipe));
    }
}