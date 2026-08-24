using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Ingredients.Actions;
using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.Shared;
using MealPlanner.Persistence;
using MealPlanner.Services.Shared;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Recipes;

public interface ICreateRecipe
{
    Task<Result<CreateRecipeResponse>> Create(CreateRecipeRequest request, CancellationToken cancellationToken);
}

public class RecipeCreator(MealPlannerDbContext ctx, 
    MeasureUnitMapper measureUnitMapper) : ICreateRecipe
{
    public async Task<Result<CreateRecipeResponse>> Create(CreateRecipeRequest request, CancellationToken cancellationToken)
    {
        var existingRecipe = await ctx.Recipes.FirstOrDefaultAsync(x => x.Name.ToLower() == request.Name.ToLower(), cancellationToken);
        if (existingRecipe is not null)
        {
            return Result.Failure<CreateRecipeResponse>(ServiceErrors.Recipe.AlreadyExists);
        }

        var mappedIngredients = await MapIngredients(request, cancellationToken);
        if (mappedIngredients.IsFailure)
        {
            return Result.Failure<CreateRecipeResponse>(mappedIngredients.Errors);
        }
            
        var stepResults = request.Steps.Select(x => RecipeStep.Create(x.Order, x.Instructions)).ToList();
        var errors = stepResults.AllErrors();
        if (errors.Count != 0)
        {
            return Result.Failure<CreateRecipeResponse>(errors);
        }
            
        var stepsByOrder = stepResults.Select(x => x.Value).OrderBy(x => x.Order).ToList();
        var result = Recipe.Create(request.Name, request.Servings, mappedIngredients.Value, stepsByOrder);
        if (result.IsFailure)
        {
            return Result.Failure<CreateRecipeResponse>(result.Errors);
        }
        
        ctx.Recipes.Add(result.Value);
        await ctx.SaveChangesAsync(cancellationToken);
        
        return Result.Success(new CreateRecipeResponse(result.Value.Id));
    }
    
    private async Task<Result<List<AddIngredientAction>>> MapIngredients(CreateRecipeRequest request, CancellationToken cancellationToken)
    {
        var incomingIngredientsIds = request.Ingredients.Select(x => x.Id).Distinct().ToList();
        var matchingIngredients = await ctx.Ingredients.Where(x => incomingIngredientsIds.Contains(x.Id)).ToListAsync(cancellationToken);
        
        if (matchingIngredients.Count != incomingIngredientsIds.Count || incomingIngredientsIds.Count == 0)
        {
            var missingIds = incomingIngredientsIds.Except(matchingIngredients.Select(x => x.Id)).ToArray();
            return Result.Failure<List<AddIngredientAction>>(ServiceErrors.Ingredient.DoesNotExist(missingIds));
        }
        
        var ingredientsToMap = matchingIngredients.ToDictionary(x => x, 
            x => request.Ingredients.Where(z => z.Id == x.Id).ToList());

        var mappedUnits = request.Ingredients.Select(x => x.Unit).Distinct().ToDictionary(x => x, measureUnitMapper.Map);
        
        var mappedIngredients = new List<AddIngredientAction>();
        var errors = new List<Error>();
        foreach (var ingredient in ingredientsToMap.Keys)
        {
            var unitsToAdd = 
                ingredientsToMap[ingredient]
                    .GroupBy(x => x.Unit)
                    .Select(group => new AddIngredientRequest(
                        ingredient.Id,
                        group.Sum(x => x.Quantity),
                        group.Key))
                    .ToList();

            var mapIngredients = MapIngredientUnits(unitsToAdd, mappedUnits, ingredient);
            if (mapIngredients.IsFailure)
            {
                errors.AddRange(mapIngredients.Errors);
            }
            else
            {
                mappedIngredients.AddRange(mapIngredients.Value);
            }
        }

        return errors.Count != 0 ? Result.Failure<List<AddIngredientAction>>(errors) : Result.Success(mappedIngredients);
    }

    private static Result<List<AddIngredientAction>> MapIngredientUnits(List<AddIngredientRequest> unitsToAdd, Dictionary<string, MeasureUnit> mappedUnits, Ingredient ingredient)
    {
        var mappedIngredients = new List<AddIngredientAction>();
        var errors = new List<Error>();
        foreach (var unitOfIngredient in unitsToAdd)
        {
            var mappedUnit = mappedUnits.GetValueOrDefault(unitOfIngredient.Unit);
            var mappedIngredient = AddIngredientAction.Create(ingredient, unitOfIngredient.Quantity, mappedUnit);
            if (mappedIngredient.IsFailure)
            {
                errors.AddRange(mappedIngredient.Errors);
            }
            else
            {
                mappedIngredients.Add(mappedIngredient.Value);
            }
        }

        return errors.Count != 0 ? Result.Failure<List<AddIngredientAction>>(errors) : Result.Success(new List<AddIngredientAction>(mappedIngredients));
    }
}