using MealPlanner.Domain;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Ingredients.Actions;
using MealPlanner.Persistence;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Recipes;

public interface ICreateRecipe
{
    Task<CreateRecipeResponse> Create(CreateRecipeRequest request, CancellationToken cancellationToken);
}

public class RecipeCreator(MealPlannerDbContext ctx, 
    MeasureUnitMapper measureUnitMapper) : ICreateRecipe
{
    public async Task<CreateRecipeResponse> Create(CreateRecipeRequest request, CancellationToken cancellationToken)
    {
        var existingRecipe = await ctx.Recipes.FirstOrDefaultAsync(x => x.Name.ToLower() == request.Name.ToLower(), cancellationToken);
        if (existingRecipe is not null)
        {
            throw new InvalidOperationException($"Recipe '{request.Name}' already exists");   // TODO: custom exceptions?
        }

        var mappedIngredients = await MapIngredients(request, cancellationToken);
        var mappedSteps = request.Steps.Select(x => RecipeStep.Create(x.Order, x.Instructions))
            .OrderBy(x => x.Order).ToList();
        
        var newRecipe = Recipe.Create(request.Name, request.Servings, mappedIngredients, mappedSteps);
        ctx.Recipes.Add(newRecipe);
        await ctx.SaveChangesAsync(cancellationToken);
        
        return new CreateRecipeResponse(newRecipe.Id);
    }
    
    // TODO: przeklikać, zaktualizować Edit menu

    private async Task<List<AddIngredientAction>> MapIngredients(CreateRecipeRequest request, CancellationToken cancellationToken)
    {
        var idsOfUsedIngredients = request.Ingredients.Select(x => x.Id).Distinct().ToList();
        var usedIngredients = await ctx.Ingredients.Where(x => idsOfUsedIngredients.Contains(x.Id)).ToListAsync(cancellationToken);
        
        if (usedIngredients.Count != idsOfUsedIngredients.Count)
        {
            throw new InvalidOperationException("One or more ingredients not found");   // TODO: custom exception?
        }
        
        var ingredientsToMap = usedIngredients.ToDictionary(x => x, 
            x => request.Ingredients.Where(z => z.Id == x.Id).ToList());

        var mappedUnits = request.Ingredients.Select(x => x.Unit).Distinct().ToDictionary(x => x, measureUnitMapper.Map);
        
        var mappedIngredients = new List<AddIngredientAction>();
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
            
            foreach (var unitOfIngredient in unitsToAdd)
            {
                var mappedUnit = mappedUnits.GetValueOrDefault(unitOfIngredient.Unit);
                var mappedIngredient = AddIngredientAction.Create(ingredient, unitOfIngredient.Quantity, mappedUnit);
                mappedIngredients.Add(mappedIngredient);
            }
        }
        
        return mappedIngredients;
    }
}