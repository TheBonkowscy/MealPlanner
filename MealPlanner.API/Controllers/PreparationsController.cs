using MealPlanner.Services;
using MealPlanner.Services.Recipes.Preparations;
using MealPlanner.Shared.Menus.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace MealPlanner.API.Controllers;

[ApiController]
[Route(Shared.Menus.Constants.PreparationsRoute)]
public class PreparationsController(
    IReadPreparations preparationsReader,
    IStringLocalizer<Translations> localizer) : ControllerBase
{
    [ProducesResponseType(typeof(GetMenuResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{date:datetime}")]
    public async Task<IResult> GetForSpecificDate([FromRoute(Name = "date")] DateTime date, CancellationToken cancellationToken)
    {
        var day = DateOnly.FromDateTime(date);
        return Results.Ok(await preparationsReader.Get(day, cancellationToken));
    }
}