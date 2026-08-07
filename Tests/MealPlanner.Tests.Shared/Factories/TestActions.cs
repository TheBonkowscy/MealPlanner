using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Ingredients.Actions;

namespace MealPlanner.Tests.Shared.Factories;

public static class TestActions
{
    public static AddIngredientAction AddIngredient(Ingredient ingredient, decimal quantity, IngredientUnit unit) => AddIngredientAction.Create(ingredient, quantity, unit);
    
    public static AddIngredientAction AddFlour(decimal quantity, IngredientUnit unit) => AddIngredientAction.Create(
        TestIngredients.CupsOfFlour(),
        quantity,
        unit);
}