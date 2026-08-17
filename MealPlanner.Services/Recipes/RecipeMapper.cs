using MealPlanner.Domain;
using MealPlanner.Shared.Recipes.Responses;

namespace MealPlanner.Services.Recipes;

public class RecipeMapper(MeasureUnitMapper measureUnitMapper)
{
    public GetRecipeDetailsResponse ToDetails(Recipe recipe)
    {
        var mappedIngredients = recipe.Ingredients.Select(x =>
            new UsedIngredientDetailsResponse(x.IngredientId, x.Ingredient.Name, x.Quantity, measureUnitMapper.Map(x.Unit))).ToList();
        var mappedSteps = recipe.Steps.Select(x => new StepDetailsResponse(x.Id, x.Order, x.Instructions)).ToList();
        return new GetRecipeDetailsResponse(recipe.Id, recipe.Name, recipe.Servings,
            mappedIngredients, 
            mappedSteps);
    }
}