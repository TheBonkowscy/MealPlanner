using MealPlanner.Services.Menus.Create;
using MealPlanner.Services.Menus.Read;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Shared.Menus.Responses;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.API.Menus;

[ApiController]
[Route(Constants.MenuRoute)]
public class MenusController(
    ICreateMenu menuCreator,
    IReadMenu menuReader) : ControllerBase
{
    [ProducesResponseType(typeof(CreateMenuResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<CreateMenuResponse> Create([FromBody] CreateMenuRequest createMenuRequest, CancellationToken cancellationToken) =>
        await menuCreator.Create(createMenuRequest);
    
    [ProducesResponseType(typeof(GetMenuResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{id:int}")]
    public async Task<IResult> GetById([FromRoute(Name = "id")] int id, CancellationToken cancellationToken)
    {
        var menu = await menuReader.Get(id);
        return menu is null ? Results.NotFound() : Results.Ok(menu);
    }
    
    [ProducesResponseType(typeof(GetMenuResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("{day:datetime}")]
    public async Task<IResult> GetForSpecificDate([FromRoute(Name = "day")] DateTime day, CancellationToken cancellationToken)
    {
        var date = DateOnly.FromDateTime(day);
        var menu = await menuReader.Get(date);
        return menu is null ? Results.NotFound() : Results.Ok(menu);
    }
    
    [ProducesResponseType(typeof(GetMenuResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpGet("today")]
    public async Task<IResult> GetForToday(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var menu = await menuReader.Get(today);
        return menu is null ? Results.NotFound() : Results.Ok(menu);
    }
}