using AwesomeAssertions;
using MealPlanner.Domain.Menus;
using MealPlanner.Domain.Recipes;
using MealPlanner.Tests.Shared.Factories;
using MealPlanner.Tests.Shared.Helpers;

namespace MealPlanner.Domain.Tests.Menus;

public class MealTests
{
    [Theory]
    [MemberData(nameof(ScalingCases))]
    public void ScaledQuantityOf_ScalesQuantityByPlannedServingsOverRecipeServings(
        decimal quantity, int recipeServings, int plannedServings, decimal expected)
    {
        // Arrange
        var recipe = RecipeWithQuantity(quantity, recipeServings);
        var meal = PlanMeal(recipe, plannedServings);

        // Act
        var scaled = meal.ScaledQuantityOf(recipe.Ingredients.Single());

        // Assert
        scaled.Should().Be(expected);
    }

    [Theory]
    [ClassData(typeof(NonPositiveNumbersTestDataProvider))]
    public void ScaledQuantityOf_TreatsNonPositiveRecipeServingsAsOne(int nonPositiveRecipeServings)
    {
        // Arrange
        var recipe = RecipeWithQuantity(100m, 1);
        typeof(Recipe).GetProperty(nameof(Recipe.Servings))!.SetValue(recipe, nonPositiveRecipeServings);
        var meal = PlanMeal(recipe, 3);

        // Act
        var scaled = meal.ScaledQuantityOf(recipe.Ingredients.Single());

        // Assert
        scaled.Should().Be(300m);
    }

    public static TheoryData<decimal, int, int, decimal> ScalingCases => new()
    {
        // quantity, recipe servings, planned servings, expected
        { 150m, 2, 4, 300m },
        { 150m, 4, 2, 75m },
        { 150m, 2, 2, 150m },
        { 2m, 4, 3, 1.5m },
        { 0.75m, 2, 4, 1.5m },
        { 3m, 3, 2, 2m },
    };

    private static Recipe RecipeWithQuantity(decimal quantity, int servings)
    {
        var recipe = TestRecipes.Create(servings: servings, ingredients: [TestIngredients.Create()]);
        recipe.Ingredients.Single().UpdateQuantity(quantity);
        return recipe;
    }

    private static Meal PlanMeal(Recipe recipe, int plannedServings)
    {
        var menu = TestMenu.Create(TestDates.Oct(1), [TestActions.AddMeal(recipe, 1, plannedServings)]);
        return menu.Meals.Single();
    }
}
