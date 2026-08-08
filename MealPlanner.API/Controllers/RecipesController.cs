using MealPlanner.Services.Recipes;
using MealPlanner.Shared.Recipes;
using MealPlanner.Shared.Recipes.Requests;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.API.Controllers;

[ApiController]
[Route(Shared.Menus.Constants.RecipesRoute)]
public class RecipesController(IReadRecipe recipeReader,
    ICreateRecipe recipeCreator) : ControllerBase
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
    
    [ProducesResponseType(typeof(CreateRecipeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<CreateRecipeResponse> Create([FromBody] CreateRecipeRequest createRecipeRequest, CancellationToken cancellationToken) =>
        await recipeCreator.Create(createRecipeRequest, cancellationToken);
}