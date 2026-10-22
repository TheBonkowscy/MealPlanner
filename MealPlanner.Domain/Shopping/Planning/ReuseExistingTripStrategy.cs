namespace MealPlanner.Domain.Shopping.Planning;

public class ReuseExistingTripStrategy : IPlanningStrategy
{
    public Task<ShoppingPlan> PlanShopping(ICollection<ShoppingNeed> productsToBuy)
    {
        throw new NotImplementedException();
    }
}