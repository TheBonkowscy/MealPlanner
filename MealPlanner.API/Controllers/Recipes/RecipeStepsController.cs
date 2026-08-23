using MealPlanner.Services;
using MealPlanner.Services.Recipes.Steps;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace MealPlanner.API.Controllers.Recipes;

[ApiController]
[Route(Shared.Menus.Constants.RecipeStepsRoute)]
public class RecipeStepsController(
    ICreateRecipeStep recipeStepCreator,
    IUpdateRecipeStep recipeStepUpdater,
    IDeleteRecipeStep recipeStepDeleter,
    IStringLocalizer<Translations> localizer) : ControllerBase
{
    [ProducesResponseType(typeof(GetRecipeDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<IResult> CreateStep(
        [FromRoute(Name = "recipeId")] int recipeId,
        CreateRecipeStepRequest request, 
        CancellationToken cancellationToken) =>
        (await recipeStepCreator.CreateStep(recipeId, request, cancellationToken)).ToHttpResult(localizer);
    
    [ProducesResponseType(typeof(GetRecipeDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPut("{stepId:int}")]
    public async Task<IResult> UpdateStep(
        [FromRoute(Name = "recipeId")] int recipeId, 
        [FromRoute(Name = "stepId")] int stepId,
        UpdateRecipeStepRequest request, 
        CancellationToken cancellationToken) =>
        (await recipeStepUpdater.UpdateStep(recipeId, stepId, request, cancellationToken)).ToHttpResult(localizer);
    
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [HttpDelete("{stepId:int}")]
    public async Task<IResult> Delete(
        [FromRoute(Name = "recipeId")] int recipeId,
        [FromRoute(Name = "stepId")] int stepId,
        CancellationToken cancellationToken)
    {
        await recipeStepDeleter.DeleteStep(recipeId, stepId, cancellationToken);
        return Results.NoContent();
    }
}