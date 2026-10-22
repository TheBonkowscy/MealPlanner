namespace MealPlanner.Domain.Shopping.Planning;

public interface IPlanningStrategy
{
    Task<ShoppingPlan> PlanShopping(ICollection<ShoppingNeed> productsToBuy);
}