using MealPlanner.Shared.Ingredients;

namespace MealPlanner.Client;

public interface IFindIngredients
{
    Task<GetIngredientsResponse> Get(CancellationToken cancellationToken = default);
}