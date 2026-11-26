using MealPlanner.Services.Menus;
using MealPlanner.Shared.Menus;
using MealPlanner.Shared.Menus.Responses;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.API.Controllers.Menus;

[ApiController]
[Route(Constants.MenusRoute)]
public class MenusController(
    IReadMenu menuReader,
    IDeleteMenu menuDeleter) : ControllerBase
{
    [ProducesResponseType(typeof(GetMenuResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id:int}")]
    public async Task<IResult> GetById([FromRoute(Name = "id")] int id, CancellationToken cancellationToken)
    {
        var menu = await menuReader.Get(id, cancellationToken);
        return menu is null ? Results.NotFound() : Results.Ok(menu);
    }
    
    [ProducesResponseType(typeof(GetMenuResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{day:datetime}")]
    public async Task<IResult> GetForSpecificDate([FromRoute(Name = "day")] DateTime day, CancellationToken cancellationToken)
    {
        var date = DateOnly.FromDateTime(day);
        var menu = await menuReader.Get(date, cancellationToken);
        return menu is null ? Results.NotFound() : Results.Ok(menu);
    }
    
    [ProducesResponseType(typeof(GetMenuResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("today")]
    public async Task<IResult> GetForToday(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var menu = await menuReader.Get(today, cancellationToken);
        return menu is null ? Results.NotFound() : Results.Ok(menu);
    }
    
    [HttpGet]
    [ProducesResponseType(typeof(GetMenuResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetForDateRange([FromQuery(Name = "from")] DateOnly? from,
        [FromQuery(Name = "to")] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var menu = await menuReader.GetRange(from, to, cancellationToken);
        return Results.Ok(menu);
    }
    
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [HttpDelete("{day:datetime}")]
    public async Task<IResult> Delete([FromRoute(Name = "day")] DateTime day, CancellationToken cancellationToken)
    {
        await menuDeleter.Delete(DateOnly.FromDateTime(day), cancellationToken);
        return Results.NoContent();
    }

    [ProducesResponseType(typeof(GetWeekResponse), StatusCodes.Status200OK)]
    [HttpGet(Constants.MenuWeeksRoute + "/{day:datetime}")]
    public async Task<IResult> GetWeek([FromRoute(Name = "day")] DateTime day, CancellationToken cancellationToken) =>
        Results.Ok(await menuReader.GetWeek(day, cancellationToken));
}