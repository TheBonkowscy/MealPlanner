using System.Collections.Concurrent;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Shopping;

namespace MealPlanner.Tests.Shared.Factories;

public static class TestShoppingNeeds
{
    // One stable ingredient id per distinct name, so needs with the same name aggregate in the planner.
    private static readonly ConcurrentDictionary<string, int> IngredientIds = new();
    private static int _lastIngredientId;

    public static ShoppingNeed Create(string name, DateOnly consumptionDate, int? shelfLifeDays,
        decimal quantity = 1, MeasureUnit unit = MeasureUnit.Piece)
    {
        var ingredientId = IngredientIds.GetOrAdd(name, _ => Interlocked.Increment(ref _lastIngredientId));

        return new ShoppingNeed(ingredientId, name, quantity, unit, shelfLifeDays, consumptionDate);
    }
}
