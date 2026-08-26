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
    ICreateRecipePreparationsInfo recipePreparationsInfoCreator,
    IUpdateRecipePreparationsInfo recipePreparationsInfoUpdater,
    IDeleteRecipePreparationsInfo recipePreparationsInfoDeleter,
    IStringLocalizer<Translations> localizer) : ControllerBase
{
    [ProducesResponseType(typeof(GetRecipeDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<IResult> CreatePreparationsInfo(
        [FromRoute(Name = "recipeId")] int recipeId,
        CreateRecipePreparationsInfoRequest request, 
        CancellationToken cancellationToken) =>
        (await recipePreparationsInfoCreator.CreatePreparations(recipeId, request, cancellationToken)).ToHttpResult(localizer);
    
    [ProducesResponseType(typeof(GetRecipeDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPut("{prepInfoId:int}")]
    public async Task<IResult> UpdateStep(
        [FromRoute(Name = "recipeId")] int recipeId, 
        [FromRoute(Name = "prepInfoId")] int prepInfoId,
        UpdateRecipePreparationsInfoRequest request, 
        CancellationToken cancellationToken) =>
        (await recipePreparationsInfoUpdater.UpdatePreparations(recipeId, prepInfoId, request, cancellationToken)).ToHttpResult(localizer);
    
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [HttpDelete("{prepInfoId:int}")]
    public async Task<IResult> DeletePreparationsInfo(
        [FromRoute(Name = "recipeId")] int recipeId,
        [FromRoute(Name = "prepInfoId")] int prepInfoId,
        CancellationToken cancellationToken)
    {
        await recipePreparationsInfoDeleter.DeletePreparations(recipeId, prepInfoId, cancellationToken);
        return Results.NoContent();
    }
}