using AwesomeAssertions;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Menus;
using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.Shopping;
using MealPlanner.Persistence;
using MealPlanner.Services.Shopping.Adapters;
using MealPlanner.Tests.Shared;
using MealPlanner.Tests.Shared.Factories;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests.Shopping;

public class MealPlanningNeedsAdapterTests
{
    private readonly List<Menu> _menus = [];
    private readonly MealPlanningNeedsAdapter _sut;

    public MealPlanningNeedsAdapterTests()
    {
        var ctx = new Mock<MealPlannerDbContext>();
        ctx.Setup(x => x.Menus).ReturnsDbSet(_menus);

        _sut = new MealPlanningNeedsAdapter(ctx.Object);
    }

    [Fact]
    public async Task GetShoppingNeeds_ScalesQuantityToPlannedServings()
    {
        // Arrange: 4 servings planned on a recipe written for 2
        var flour = IngredientOf("Flour", MeasureUnit.Gram);
        PlanMenu(TestDates.Oct(5), (RecipeOf(2, (flour, 150m)), 4));

        // Act
        var needs = await _sut.GetShoppingNeeds(TestDates.Oct(2), TestDates.Oct(8), CancellationToken.None);

        // Assert
        needs.Should().ContainSingle().Which.Quantity.Should().Be(300m);
    }

    [Fact]
    public async Task GetShoppingNeeds_ScalesEachMealByItsOwnServings_AndIncludesEveryIngredientOfEveryMeal()
    {
        // Arrange: two meals on one day, each with its own recipe servings and planned servings
        var flour = IngredientOf("Flour", MeasureUnit.Gram);
        var sugar = IngredientOf("Sugar", MeasureUnit.Gram);
        var milk = IngredientOf("Milk", MeasureUnit.Milliliter);
        var date = TestDates.Oct(5);
        PlanMenu(date,
            (RecipeOf(2, (flour, 100m), (sugar, 10m)), 4),
            (RecipeOf(1, (milk, 50m)), 3));

        // Act
        var needs = await _sut.GetShoppingNeeds(TestDates.Oct(2), TestDates.Oct(8), CancellationToken.None);

        // Assert
        needs.Should().BeEquivalentTo(
        [
            new ShoppingNeed(flour.Id, "Flour", 200m, MeasureUnit.Gram, null, date),
            new ShoppingNeed(sugar.Id, "Sugar", 20m, MeasureUnit.Gram, null, date),
            new ShoppingNeed(milk.Id, "Milk", 150m, MeasureUnit.Milliliter, null, date),
        ]);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(null)]
    public async Task GetShoppingNeeds_MapsIngredientDetailsAndMealDate(int? shelfLifeDays)
    {
        // Arrange
        var tomato = IngredientOf("Tomato", MeasureUnit.Piece, shelfLifeDays);
        var date = TestDates.Oct(6);
        PlanMenu(date, (RecipeOf(1, (tomato, 3m)), 1));

        // Act
        var needs = await _sut.GetShoppingNeeds(TestDates.Oct(2), TestDates.Oct(8), CancellationToken.None);

        // Assert
        needs.Should().ContainSingle().Which.Should().BeEquivalentTo(
            new ShoppingNeed(tomato.Id, "Tomato", 3m, MeasureUnit.Piece, shelfLifeDays, date));
    }

    [Fact]
    public async Task GetShoppingNeeds_IncludesOnlyMenusWithinTheRange_BoundsInclusive()
    {
        // Arrange: the range is F+1 ... F+7 for a start day F = 1 October, as the generator calls it
        var flour = IngredientOf("Flour", MeasureUnit.Gram);
        foreach (var day in new[] { 1, 2, 5, 8, 9 })
        {
            PlanMenu(TestDates.Oct(day), (RecipeOf(1, (flour, 100m)), 1));
        }

        // Act
        var needs = await _sut.GetShoppingNeeds(TestDates.Oct(2), TestDates.Oct(8), CancellationToken.None);

        // Assert: 1 October (the start day) and 9 October (after the cycle) are left out; days without a menu add nothing
        needs.Select(x => x.ConsumptionDate).Should().BeEquivalentTo(TestDates.Oct([2, 5, 8]));
    }

    [Fact]
    public async Task GetShoppingNeeds_ReturnsNothing_WhenNoMenuIsPlannedInTheRange()
    {
        // Arrange
        var flour = IngredientOf("Flour", MeasureUnit.Gram);
        PlanMenu(TestDates.Oct(20), (RecipeOf(1, (flour, 100m)), 1));

        // Act
        var needs = await _sut.GetShoppingNeeds(TestDates.Oct(2), TestDates.Oct(8), CancellationToken.None);

        // Assert
        needs.Should().BeEmpty();
    }

    private void PlanMenu(DateOnly date, params (Recipe Recipe, int Servings)[] meals)
    {
        var mealsToAdd = meals.Select((meal, index) => TestActions.AddMeal(meal.Recipe, index + 1, meal.Servings)).ToList();
        var menu = TestMenu.Create(date, mealsToAdd);
        RandomId.Set(menu);
        _menus.Add(menu);
    }

    private static Ingredient IngredientOf(string name, MeasureUnit unit, int? shelfLifeDays = null)
    {
        var ingredient = TestIngredients.Create(name, [unit], shelfLifeDays);
        RandomId.Set(ingredient);
        return ingredient;
    }

    private static Recipe RecipeOf(int servings, params (Ingredient Ingredient, decimal Quantity)[] used)
    {
        var recipe = TestRecipes.Create(servings: servings, ingredients: [.. used.Select(x => x.Ingredient)]);
        foreach (var (ingredient, quantity) in used)
        {
            recipe.Ingredients.Single(x => x.Ingredient == ingredient).UpdateQuantity(quantity);
        }

        return recipe;
    }
}
