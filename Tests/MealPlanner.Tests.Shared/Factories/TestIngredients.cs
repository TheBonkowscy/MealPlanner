using MealPlanner.Domain.Ingredients;

namespace MealPlanner.Tests.Shared.Factories;

public static class TestIngredients
{
    public const string IngredientName = "Flour";
    public static Ingredient NewIngredient(string name, List<IngredientUnit> applicableUnits) => Ingredient.Create(name, applicableUnits);

    public static Ingredient CupsOfFlour() => NewIngredient(IngredientName, [TestIngredientsUnits.Cups()]);
}