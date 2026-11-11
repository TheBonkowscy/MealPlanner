using MealPlanner.Domain.Shared;
using MealPlanner.Domain.Shopping;
using MealPlanner.Domain.Shopping.Planning;
using MealPlanner.Services.Recipes;
using MealPlanner.Shared.Shopping;
using MealPlanner.Shared.Shopping.Responses;

namespace MealPlanner.Services.Shopping;

public class ShoppingListGenerator(
    IShoppingNeedsSource shoppingNeedsSource,
    IClosedDaysProvider closedDaysProvider,
    IPlanShoppingTrips shoppingTripPlanner,
    MeasureUnitMapper measureUnitMapper)
{
    public async Task<Result<ShoppingListResponse>> Generate(DateOnly from, PlanningStrategyKind planningStrategy, CancellationToken ct)
    {
        var aWeekLater = from.AddDays(7);
        var sixDaysBefore = from.AddDays(-6);
        var needsFrom = from.AddDays(1);
        var publicHolidays = await closedDaysProvider.GetClosedDays(sixDaysBefore, aWeekLater, ct);
        if (publicHolidays.IsFailure)
        {
            return Result.Failure<ShoppingListResponse>(publicHolidays.Errors);
        }

        var itemsToBuy = await shoppingNeedsSource.GetShoppingNeeds(needsFrom, aWeekLater, ct);
        if (itemsToBuy.IsFailure)
        {
            return Result.Failure<ShoppingListResponse>(itemsToBuy.Errors);
        }

        var strategy = PlanningStrategyFactory.CreatePlanningStrategy(planningStrategy);
        var plannedTrips = shoppingTripPlanner.PlanTrips(from, itemsToBuy.Value, publicHolidays.Value, strategy);
        return plannedTrips.IsFailure 
            ? Result.Failure<ShoppingListResponse>(plannedTrips.Errors) 
            : Result.Success(MapResponse(plannedTrips.Value, planningStrategy));
    }

    private ShoppingListResponse MapResponse(ShoppingTrips plannedTrips, PlanningStrategyKind strategy)
    {
        return new ShoppingListResponse(plannedTrips.StartDate,
            strategy,
        [.. plannedTrips.Trips.Select(MapPlannedTrip)]);
    }

    private PlannedTripResponse MapPlannedTrip(PlannedTrip plannedTrip)
    {
        return new PlannedTripResponse(plannedTrip.Date, [.. plannedTrip.Items.Select(MapPlannedItem)]);
    }

    private PlannedItemResponse MapPlannedItem(PlannedItem plannedItem)
    {
        return new PlannedItemResponse(plannedItem.IngredientName,
            measureUnitMapper.Map(plannedItem.Unit),
            plannedItem.Quantity,
            plannedItem.ShelfLifeWarning);
    }
}