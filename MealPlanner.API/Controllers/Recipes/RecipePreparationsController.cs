using MealPlanner.Services;
using MealPlanner.Services.Recipes.Preparations;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace MealPlanner.API.Controllers.Recipes;

[ApiController]
[Route(Shared.Menus.Constants.RecipePreparationsRoute)]
public class RecipePreparationsController(
    ICreateRecipePreparations recipePreparationsCreator,
    IUpdateRecipePreparations recipePreparationsUpdater,
    IDeleteRecipePreparations recipePreparationsDeleter,
    IStringLocalizer<Translations> localizer) : ControllerBase
{
    [ProducesResponseType(typeof(GetRecipeDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<IResult> CreatePreparations(
        [FromRoute(Name = "recipeId")] int recipeId,
        CreateRecipePreparationsRequest request, 
        CancellationToken cancellationToken) =>
        (await recipePreparationsCreator.CreatePreparations(recipeId, request, cancellationToken)).ToHttpResult(localizer);
    
    [ProducesResponseType(typeof(GetRecipeDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPut("{prepInfoId:int}")]
    public async Task<IResult> UpdatePreparations(
        [FromRoute(Name = "recipeId")] int recipeId, 
        [FromRoute(Name = "prepInfoId")] int prepInfoId,
        UpdateRecipePreparationsRequest request, 
        CancellationToken cancellationToken) =>
        (await recipePreparationsUpdater.UpdatePreparations(recipeId, prepInfoId, request, cancellationToken)).ToHttpResult(localizer);
    
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [HttpDelete("{prepInfoId:int}")]
    public async Task<IResult> DeletePreparations(
        [FromRoute(Name = "recipeId")] int recipeId,
        [FromRoute(Name = "prepInfoId")] int prepInfoId,
        CancellationToken cancellationToken)
    {
        await recipePreparationsDeleter.DeletePreparations(recipeId, prepInfoId, cancellationToken);
        return Results.NoContent();
    }
}