using MealPlanner.Services.Recipes;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.API.Controllers.Recipes;

[ApiController]
[Route(Shared.Menus.Constants.RecipesRoute)]
public class RecipesController(IReadRecipe recipeReader,
    ICreateRecipe recipeCreator,
    IDeleteRecipe recipeDeleter) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(GetRecipesResponse), StatusCodes.Status200OK)]
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
    
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(GetRecipesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> Get([FromRoute(Name = "id")] int id, CancellationToken cancellationToken)
    {
        var recipe = await recipeReader.Get(id, cancellationToken);
        return recipe is null ? Results.NotFound() : Results.Ok(recipe);
    }
    
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [HttpDelete("{id:int}")]
    public async Task<IResult> Delete([FromRoute(Name = "id")] int id, CancellationToken cancellationToken)
    {
        await recipeDeleter.Delete(id, cancellationToken);
        return Results.NoContent();
    }
}