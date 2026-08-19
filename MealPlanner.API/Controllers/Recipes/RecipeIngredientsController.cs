using MealPlanner.Services.Recipes;
using MealPlanner.Services.Recipes.Ingredients;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.API.Controllers.Recipes;

[ApiController]
[Route(Shared.Menus.Constants.RecipeIngredientsRoute)]
public class RecipeIngredientsController(
    IUpdateRecipeIngredient recipeIngredientUpdater,
    IDeleteRecipeIngredient recipeIngredientDeleter) : ControllerBase
{
    [ProducesResponseType(typeof(GetRecipeDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPut("{ingredientId:int}")]
    public async Task<GetRecipeDetailsResponse> UpdateIngredient(
        [FromRoute(Name = "recipeId")] int recipeId, 
        [FromRoute(Name = "ingredientId")] int ingredientId,
        UpdateRecipeIngredientRequest request, 
        CancellationToken cancellationToken) =>
        await recipeIngredientUpdater.UpdateIngredient(recipeId, ingredientId, request, cancellationToken);
    
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [HttpDelete("{ingredientId:int}")]
    public async Task<IResult> Delete(
        [FromRoute(Name = "recipeId")] int recipeId,
        [FromRoute(Name = "ingredientId")] int ingredientId,
        [FromBody] DeleteRecipeIngredientRequest request,
        CancellationToken cancellationToken)
    {
        await recipeIngredientDeleter.DeleteIngredient(recipeId, ingredientId, request.Unit, cancellationToken);
        return Results.NoContent();
    }
}