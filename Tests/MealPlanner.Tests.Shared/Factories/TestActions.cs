using MealPlanner.Domain;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Ingredients.Actions;
using MealPlanner.Domain.Menus.Actions;

namespace MealPlanner.Tests.Shared.Factories;

public static class TestActions
{
    public static AddIngredientAction AddIngredient(Ingredient ingredient, decimal quantity, MeasureUnit unit) => AddIngredientAction.Create(ingredient, quantity, unit);
    
    public static AddIngredientAction AddFlour(decimal quantity, MeasureUnit unit) => AddIngredientAction.Create(
        TestInitialData.CupsOfFlour(),
        quantity,
        unit);

    public static AddMealAction AddMeal(Recipe recipe, int order, int servings) =>
        AddMealAction.Create(recipe, order, servings);
}