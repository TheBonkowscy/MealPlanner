using MealPlanner.Services.Recipes.Read;
using MealPlanner.Shared.Menus.Responses;
using MealPlanner.Shared.Recipes;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.API.Menus;

[ApiController]
[Route(Shared.Menus.Constants.RecipesRoute)]
public class RecipesController(IReadRecipe recipeReader) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(GetRecipesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetByQuery([FromQuery(Name = "q")] string? query,
        CancellationToken cancellationToken)
    {
        var recipe = await recipeReader.GetByQuery(query, cancellationToken);
        return Results.Ok(recipe);
    }
}