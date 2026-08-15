using MealPlanner.Services.Menus;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Shared.Menus.Responses;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.API.Controllers;

[ApiController]
[Route(Shared.Menus.Constants.MenusRoute)]
public class MenusController(
    ICreateMenu menuCreator,
    IReadMenu menuReader,
    IUpdateMenu menusUpdater,
    IDeleteMenu menuDeleter) : ControllerBase
{
    [ProducesResponseType(typeof(CreateMenuResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<CreateMenuResponse> Create([FromBody] CreateMenuRequest createMenuRequest, CancellationToken cancellationToken) =>
        await menuCreator.Create(createMenuRequest, cancellationToken);
    
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
    
    [ProducesResponseType(typeof(GetMenuResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> GetForDateRange([FromQuery(Name = "from")] DateOnly? from,
        [FromQuery(Name = "to")] DateOnly? to,
        CancellationToken cancellationToken)
    {
        var menu = await menuReader.GetRange(from, to, cancellationToken);
        return Results.Ok(menu);
    }
    
    [ProducesResponseType(typeof(UpdateMenuResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPut("{day:datetime}")]
    public async Task<UpdateMenuResponse> Update([FromRoute(Name = "day")] DateTime day,
        [FromBody] UpdateMenuRequest updateMenuRequest,
        CancellationToken cancellationToken) =>
        await menusUpdater.Update(updateMenuRequest, cancellationToken);
    
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [HttpDelete("{day:datetime}")]
    public async Task<IResult> Delete([FromRoute(Name = "day")] DateTime day, CancellationToken cancellationToken)
    {
        await menuDeleter.Delete(DateOnly.FromDateTime(day), cancellationToken);
        return Results.NoContent();
    }
}