using MealPlanner.Domain.Ingredients;

namespace MealPlanner.Tests.Shared.Factories;

public static class TestIngredientsUnits
{
    public static IngredientUnit Cups() => IngredientUnit.Create("Cups");
    
    public static IngredientUnit Unit(string name) => IngredientUnit.Create(name);
}