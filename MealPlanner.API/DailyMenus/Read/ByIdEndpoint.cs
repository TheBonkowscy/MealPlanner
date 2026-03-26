using MealPlanner.Services.DailyMenus.Read;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.API.DailyMenus.Read;

public static class ByIdEndpoint
{
    public const string Address = Constants.EndpointPrefix + "/{id:guid}";
    
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> Read(
        [FromServices] IReadDailyMenu dailyMenuReader,
        [FromRoute(Name = "id")] Guid id)
    {
        var dailyMenu = await dailyMenuReader.Get(id);
        return dailyMenu is null ? Results.NotFound() : Results.Ok(dailyMenu);
    }
}