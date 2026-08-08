using MealPlanner.Domain.Ingredients;

namespace MealPlanner.Tests.Shared.Factories;

public static class TestIngredientsUnits
{
    public static MeasureUnit Cups() => MeasureUnit.Create("Cups");
    
    public static MeasureUnit Unit(string name) => MeasureUnit.Create(name);
}