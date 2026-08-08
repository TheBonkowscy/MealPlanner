using MealPlanner.Services.Meals.Read;
using MealPlanner.Shared.Meals;
using MealPlanner.Shared.Menus.Responses;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.API.Menus;

[ApiController]
[Route(Shared.Menus.Constants.RecipesRoute)]
public class RecipesController(IReadRecipes recipesReader) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(GetRecipesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetByQuery([FromQuery(Name = "q")] string? query,
        CancellationToken cancellationToken)
    {
        var recipe = await recipesReader.GetByQuery(query, cancellationToken);
        return Results.Ok(recipe);
    }
}