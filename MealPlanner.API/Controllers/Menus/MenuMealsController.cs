using MealPlanner.Services;
using MealPlanner.Services.Menus.Meals;
using MealPlanner.Shared.Menus;
using MealPlanner.Shared.Menus.Meals;
using MealPlanner.Shared.Menus.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace MealPlanner.API.Controllers.Menus;

[ApiController]
[Route(Constants.MenuMealsRoute)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public class MenuMealsController(
    ICreateMeals mealCreator,
    IUpdateMeals mealUpdater,
    IDeleteMeals mealDeleter,
    ITransferMeals mealTransferer,
    IStringLocalizer<Translations> localizer) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> AddMeal([FromRoute(Name = "date")] DateTime date,
        [FromBody] CreateMealRequest request,
        CancellationToken token)
    {
        var result = await mealCreator.Create(DateOnly.FromDateTime(date), request, token);
        return result.ToHttpResult(localizer);
    }

    [HttpPut("{mealId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> UpdateMeal([FromRoute(Name = "date")] DateTime date,
        [FromRoute(Name = "mealId")] int mealId,
        [FromBody] UpdateMealRequest request,
        CancellationToken token)
    {
        var result = await mealUpdater.Update(DateOnly.FromDateTime(date), mealId, request, token);
        return result.ToHttpResult(localizer);
    }

    [HttpDelete("{mealId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> DeleteMeal([FromRoute(Name = "date")] DateTime date,
        [FromRoute(Name = "mealId")] int mealId,
        CancellationToken token)
    {
        var result = await mealDeleter.Delete(DateOnly.FromDateTime(date), mealId, token);
        return result.ToHttpResult(localizer);
    }

    [HttpPost("{mealId:int}/transfer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IResult> TransferMeal([FromRoute(Name = "date")] DateTime date,
        [FromRoute(Name = "mealId")] int mealId,
        [FromBody] TransferMealRequest request,
        CancellationToken token)
    {
        var result = await mealTransferer.Transfer(DateOnly.FromDateTime(date), mealId, request, token);
        return result.ToHttpResult(localizer);
    }
}