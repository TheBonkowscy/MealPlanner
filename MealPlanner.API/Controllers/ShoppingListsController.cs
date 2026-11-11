using MealPlanner.Services;
using MealPlanner.Services.Shopping;
using MealPlanner.Shared.Shopping;
using MealPlanner.Shared.Shopping.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;

namespace MealPlanner.API.Controllers;

[ApiController]
[Route(Constants.ShoppingListsRoute)]
public class ShoppingListsController(
    ShoppingListGenerator shoppingListGenerator,
    IStringLocalizer<Translations> localizer) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ShoppingListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IResult> Get(
        [FromQuery, BindRequired] DateOnly startDate,
        [FromQuery] PlanningStrategyKind strategy = PlanningStrategyKind.ReuseExistingTrip,
        CancellationToken cancellationToken = default)
    {
        var result = await shoppingListGenerator.Generate(startDate, strategy, cancellationToken);
        return result.ToHttpResult(localizer);
    }
}