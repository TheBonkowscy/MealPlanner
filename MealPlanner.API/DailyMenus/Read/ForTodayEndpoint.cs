using MealPlanner.Services.DailyMenus.Read;
using Microsoft.AspNetCore.Mvc;

namespace MealPlanner.API.DailyMenus.Read;

public static class ForTodayEndpoint
{
    public const string Address = Constants.EndpointPrefix + "/today";
    
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public static async Task<IResult> Read([FromServices] IReadDailyMenu dailyMenuReader)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dailyMenu = await dailyMenuReader.Get(today);
        return dailyMenu is null ? Results.NotFound() : Results.Ok(dailyMenu);
    }
}