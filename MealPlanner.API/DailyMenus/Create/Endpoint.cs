using MealPlanner.Services.DailyMenus.Create;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.API.DailyMenus.Create;

public static class Endpoint
{
    public const string Address = Constants.EndpointPrefix;
    
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public static async Task<Response> Create(
        [FromServices] ICreateDailyMenu dailyMenuCreator,
        [FromBody] Request request) =>
        await dailyMenuCreator.Create(request);
}