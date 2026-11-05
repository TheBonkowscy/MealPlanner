using MealPlanner.Domain.Shopping;

namespace MealPlanner.Services.Shopping;

public interface IShoppingNeedsSource
{
    Task<List<ShoppingNeed>> GetShoppingNeeds(DateOnly from, DateOnly to, CancellationToken ct);
}