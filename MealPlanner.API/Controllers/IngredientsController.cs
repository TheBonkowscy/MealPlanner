using MealPlanner.Services.Ingredients;
using MealPlanner.Shared.Ingredients;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.API.Controllers;

[ApiController]
[Route(Shared.Menus.Constants.IngredientsRoute)]
public class IngredientsController(IReadIngredient ingredientsReader) : ControllerBase
{
 
    [HttpGet]
    [ProducesResponseType(typeof(GetIngredientsResponse), StatusCodes.Status200OK)]
    public async Task<IResult> Get(CancellationToken cancellationToken)
    {
        var ingredients = await ingredientsReader.Get(cancellationToken);
        return Results.Ok(ingredients);
    }
}