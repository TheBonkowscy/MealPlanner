using MealPlanner.Services.Meals.Read;
using MealPlanner.Shared.Meals;
using MealPlanner.Shared.Menus.Responses;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.API.Menus;

[ApiController]
[Route(Shared.Menus.Constants.MealsRoute)]
public class MealsController(IReadMeals mealsReader) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(GetMealsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetByQuery([FromQuery(Name = "q")] string? query,
        CancellationToken cancellationToken)
    {
        var menu = await mealsReader.GetByQuery(query, cancellationToken);
        return Results.Ok(menu);
    }
}