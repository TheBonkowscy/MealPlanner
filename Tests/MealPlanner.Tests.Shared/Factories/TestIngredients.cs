using MealPlanner.Domain.Ingredients;

namespace MealPlanner.Tests.Shared.Factories;

public static class TestIngredients
{
    public static Ingredient Create(string? name = null, 
        List<MeasureUnit>? units = null,
        int? shelfLife = null)
    {
        name ??= $"Ingredient_{Guid.NewGuid().ToString()}";
        units ??= [.. Enum.GetValues<MeasureUnit>()];

        return shelfLife is null
            ? Ingredient.Create(name, units).Value
            : Ingredient.Create(name, shelfLife.Value, units).Value;
    }
}