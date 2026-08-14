using MealPlanner.Domain.Ingredients;

namespace MealPlanner.Tests.Shared.Factories;

public static class TestIngredients
{
    public static Ingredient Create(string? name = null, 
        List<MeasureUnit>? units = null)
    {
        name ??= $"Ingredient_{Guid.NewGuid().ToString()}";
        units ??= [.. Enum.GetValues<MeasureUnit>()];
        
        return Ingredient.Create(name, units);
    }
}