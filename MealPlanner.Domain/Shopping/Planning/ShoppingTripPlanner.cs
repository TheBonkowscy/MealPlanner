using MealPlanner.Domain.Shared;

namespace MealPlanner.Domain.Shopping.Planning;

public interface IPlanShoppingTrips
{
    Result<ShoppingTrips> PlanTrips(DateOnly start,
        List<ShoppingNeed> itemsToBuy,
        IReadOnlySet<DateOnly> publicHolidays,
        IPlanningStrategy strategy);
}

public class ShoppingTripPlanner : IPlanShoppingTrips
{
    private record AssignmentDto(ShoppingNeed Need, bool Warning);
    
    public Result<ShoppingTrips> PlanTrips(DateOnly start,
        List<ShoppingNeed> itemsToBuy,
        IReadOnlySet<DateOnly> publicHolidays,
        IPlanningStrategy strategy)
    {
        var availableDays = Enumerable.Range(0, ShoppingSchedule.CycleLengthInDays + 1)
            .Select(start.AddDays)
            .Except(publicHolidays)
            .Where(x => ShoppingSchedule.FreshShopDays.Contains(x.DayOfWeek))
            .ToList();

        var bigTrip = FindBigTrip(start, publicHolidays);
        if (bigTrip.IsFailure)
        {
            return Result.Failure<ShoppingTrips>(bigTrip.Error);
        }

        if (!availableDays.Contains(bigTrip.Value))
        {
            availableDays.Add(bigTrip.Value);
        }

        var end = start.AddDays(ShoppingSchedule.CycleLengthInDays);
        var itemsNeededInCycle = itemsToBuy.Where(x => x.ConsumptionDate > start && x.ConsumptionDate <= end).ToList();
        
        var durableGoods = itemsNeededInCycle.Where(x => x.ShelfLifeDays is null).ToList();
        var perishableGoods = itemsNeededInCycle.Where(x => x.ShelfLifeDays is not null)
            .OrderBy(x => x.ConsumptionDate)
            .ThenBy(x => x.IngredientName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Unit).ToList();
        
        var shoppingTrips = new Dictionary<DateOnly, List<AssignmentDto>>();
        if (durableGoods.Count != 0)
        {
            shoppingTrips[bigTrip.Value] = [.. durableGoods.Select(x => new AssignmentDto(x, x.ConsumptionDate < bigTrip.Value))];
        }

        foreach (var good in perishableGoods)
        {
            var allowedTripsForItem = FindAllowedTrips(good,  bigTrip.Value, availableDays, publicHolidays);
            DateOnly tripDate;
            AssignmentDto assignment;
            
            if (allowedTripsForItem.Count > 0)
            {
                tripDate = strategy.Choose(allowedTripsForItem, shoppingTrips.Keys.ToHashSet());
                assignment = new AssignmentDto(good, false);
            }
            else
            {
                var earlierTrips = availableDays.Where(x => x <= GetLatestTripFor(good, bigTrip.Value));
                tripDate = earlierTrips.DefaultIfEmpty(bigTrip.Value).Max();
                assignment = new AssignmentDto(good, true);
            }

            if (!shoppingTrips.ContainsKey(tripDate))
            {
                shoppingTrips[tripDate] = [];
            }

            shoppingTrips[tripDate].Add(assignment);
        }
        
        var trips = new ShoppingTrips(start, BuildTrips(shoppingTrips));
        return Result.Success(trips);
    }

    private Result<DateOnly> FindBigTrip(DateOnly start, IReadOnlySet<DateOnly> publicHolidays)
    {
        if (!publicHolidays.Contains(start))
        {
            return Result.Success(start);
        }
        
        for (var i = 1; i < ShoppingSchedule.MaxBigTripShiftInDays; i++)
        {
            var previous = start.AddDays(-i);
            var next = start.AddDays(i);

            if (!publicHolidays.Contains(previous))
            {
                return Result.Success(previous);
            }

            if (!publicHolidays.Contains(next))
            {
                return Result.Success(next);
            }
        }
        
        return Result.Failure<DateOnly>(DomainErrors.Shopping.NoOpenBigShopDay(start));
    }

    private static IReadOnlyCollection<DateOnly> FindAllowedTrips(ShoppingNeed good,
        DateOnly bigTrip,
        List<DateOnly> availableDays,
        IReadOnlySet<DateOnly> publicHolidays)
    {
        var shelfLife = good.ShelfLifeDays;
        if (shelfLife == 0)
        {
            return publicHolidays.Contains(good.ConsumptionDate) ? [] : [good.ConsumptionDate];
        }
        
        var earliestDay = good.ConsumptionDate.DayNumber - (long)shelfLife!;
        var latestDate = GetLatestTripFor(good, bigTrip); 
        
        return [.. availableDays.Where(x => x.DayNumber >= earliestDay && x <= latestDate)];
    }

    private static DateOnly GetLatestTripFor(ShoppingNeed good, DateOnly bigTrip) => good.ConsumptionDate == bigTrip
        ? good.ConsumptionDate
        : good.ConsumptionDate.AddDays(-1);

    private static IReadOnlyList<PlannedTrip> BuildTrips(Dictionary<DateOnly, List<AssignmentDto>> shoppingTrips)
    {
        var trips = new List<PlannedTrip>();
        foreach (var trip in shoppingTrips.OrderBy(x => x.Key))
        {
            var items = trip.Value.GroupBy(x => (x.Need.IngredientId, x.Need.Unit))
                .Select(x => new PlannedItem(x.Key.IngredientId,
                    x.First().Need.IngredientName,
                    x.Key.Unit,
                    x.Sum(z => z.Need.Quantity),
                    x.Any(f => f.Warning)))
                .OrderBy(x => x.IngredientName, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(x => x.Unit)
                .ToList();
            trips.Add(new PlannedTrip(trip.Key, items));
        }
        return trips;
    }
}