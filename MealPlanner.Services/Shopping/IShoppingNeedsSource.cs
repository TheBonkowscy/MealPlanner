using MealPlanner.Domain.Shared;
using MealPlanner.Domain.Shopping;

namespace MealPlanner.Services.Shopping;

public interface IShoppingNeedsSource
{
    Task<Result<List<ShoppingNeed>>> GetShoppingNeeds(DateOnly from, DateOnly to, CancellationToken ct);
}