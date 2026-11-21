using AwesomeAssertions;
using MealPlanner.Domain.Menus;
using MealPlanner.Domain.Shared;
using MealPlanner.Tests.Shared.Factories;
using MealPlanner.Tests.Shared.Helpers;

namespace MealPlanner.Domain.Tests.Menus;

public class MenuMealOperationsTests
{
    [Theory]
    [InlineData(1, new[] { 4, 1, 2, 3 })]
    [InlineData(2, new[] { 1, 4, 2, 3 })]
    [InlineData(4, new[] { 1, 2, 3, 4 })]
    public void InsertMeal_ProducesExpectedLayout(int position, int[] expectedRecipeIds)
    {
        // Arrange
        var menu = MenuWith(1, 2, 3);

        // Act
        var result = menu.InsertMeal(TestActions.AddMeal(TestRecipes.CreateWithId(4), position, 1));

        // Assert
        result.IsSuccess.Should().BeTrue();
        AssertLayout(menu, expectedRecipeIds);
    }

    [Fact]
    public void InsertMeal_StoresTheGivenServings()
    {
        // Arrange
        var menu = MenuWith(1);

        // Act
        menu.InsertMeal(TestActions.AddMeal(TestRecipes.CreateWithId(2), 2, 3));

        // Assert
        menu.Meals.Single(m => m.RecipeId == 2).Servings.Should().Be(3);
    }

    // AddMeal accepts any position on an empty menu; InsertMeal must not inherit that.
    [Fact]
    public void InsertMeal_IntoEmptyMenu_Fails_WhenPositionIsAboveOne()
    {
        // Arrange
        var menu = TestMenu.Create(TestDates.Oct(1), []);

        // Act
        var result = menu.InsertMeal(TestActions.AddMeal(TestRecipes.CreateWithId(1), 2, 1));

        // Assert
        result.Errors.Should().Contain(DomainErrors.Menu.InvalidMealOrder);
        menu.Meals.Should().BeEmpty();
    }

    [Fact]
    public void InsertMeal_IntoEmptyMenu_Succeeds_AtPositionOne()
    {
        // Arrange
        var menu = TestMenu.Create(TestDates.Oct(1), []);

        // Act
        var result = menu.InsertMeal(TestActions.AddMeal(TestRecipes.CreateWithId(1), 1, 1));

        // Assert
        result.IsSuccess.Should().BeTrue();
        AssertLayout(menu, 1);
    }

    [Fact]
    public void InsertMeal_Fails_WhenPositionIsBeyondTheEnd_AndChangesNothing()
    {
        // Arrange
        var menu = MenuWith(1, 2);

        // Act
        var result = menu.InsertMeal(TestActions.AddMeal(TestRecipes.CreateWithId(3), 4, 1));

        // Assert
        result.Errors.Should().Contain(DomainErrors.Menu.InvalidMealOrder);
        AssertLayout(menu, 1, 2);
    }

    // Position 1 is valid here, so only a shift done before the duplicate check can leave the layout changed.
    [Fact]
    public void InsertMeal_Fails_WhenRecipeIsAlreadyInTheDay_AndChangesNothing()
    {
        // Arrange
        var existing = TestRecipes.CreateWithId(1);
        var menu = TestMenu.Create(TestDates.Oct(1),
            [TestActions.AddMeal(existing, 1, 1), TestActions.AddMeal(TestRecipes.CreateWithId(2), 2, 1)]);

        // Act
        var result = menu.InsertMeal(TestActions.AddMeal(existing, 1, 3));

        // Assert
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Meal.AlreadyPresentInTheDay(existing.Name));
        AssertLayout(menu, 1, 2);
    }

    // AddMeal lets a menu be built with orders (3, 1): a gap at 2, and the list not sorted by order.
    // Positions are ranks: recipe 20 is first, recipe 10 second. Position 2 is the one place where the new
    // meal fits into the gap by accident, so positions 1 and 3 are what prove that the orders are renumbered.
    [Theory]
    [InlineData(1, new[] { 30, 20, 10 })]
    [InlineData(2, new[] { 20, 30, 10 })]
    [InlineData(3, new[] { 20, 10, 30 })]
    public void InsertMeal_TreatsOrdersAsRanks_WhenOrdersAreUnsortedAndHaveAGap(int position, int[] expectedRecipeIds)
    {
        // Arrange
        var menu = MenuWithOrders((10, 3), (20, 1));

        // Act
        var result = menu.InsertMeal(TestActions.AddMeal(TestRecipes.CreateWithId(30), position, 1));

        // Assert
        result.IsSuccess.Should().BeTrue();
        AssertLayout(menu, expectedRecipeIds);
    }

    // A single meal stored with order 5 is still the first (and only) meal, so position 2 means "after it".
    // Comparing the position with the stored order would put the new meal before it.
    [Theory]
    [InlineData(1, new[] { 30, 10 })]
    [InlineData(2, new[] { 10, 30 })]
    public void InsertMeal_TreatsOrdersAsRanks_WhenTheOnlyMealHasAHigherOrder(int position, int[] expectedRecipeIds)
    {
        // Arrange
        var menu = MenuWithOrders((10, 5));

        // Act
        var result = menu.InsertMeal(TestActions.AddMeal(TestRecipes.CreateWithId(30), position, 1));

        // Assert
        result.IsSuccess.Should().BeTrue();
        AssertLayout(menu, expectedRecipeIds);
    }

    // Recipe ids (11, 22, 33) differ from the positions (1, 2, 3) on purpose: a lookup by position
    // instead of by recipe id would remove the wrong meal. Removing the first and the middle meal also
    // proves that the remaining orders are renumbered to 1..n.
    [Theory]
    [InlineData(11, new[] { 22, 33 })]
    [InlineData(22, new[] { 11, 33 })]
    [InlineData(33, new[] { 11, 22 })]
    public void RemoveMeal_ProducesExpectedLayout(int recipeId, int[] expectedRecipeIds)
    {
        // Arrange
        var menu = MenuWith(11, 22, 33);

        // Act
        var result = menu.RemoveMeal(recipeId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        AssertLayout(menu, expectedRecipeIds);
    }

    [Fact]
    public void RemoveMeal_OfTheLastMeal_LeavesAnEmptyMenu()
    {
        // Arrange
        var menu = MenuWith(11);

        // Act
        var result = menu.RemoveMeal(11);

        // Assert
        result.IsSuccess.Should().BeTrue();
        menu.Meals.Should().BeEmpty();
    }

    // Recipe id 1 equals the position of the first meal but is not a recipe in this day,
    // so a lookup by position would wrongly find a meal here.
    [Fact]
    public void RemoveMeal_Fails_WhenMealIsNotInTheDay_AndChangesNothing()
    {
        // Arrange
        var menu = MenuWith(11, 22);

        // Act
        var result = menu.RemoveMeal(1);

        // Assert
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Meal.NotFoundInTheDay(1));
        result.Error.Type.Should().Be(ErrorType.NotFound);
        AssertLayout(menu, 11, 22);
    }

    // Orders (3, 1): recipe 20 is first. Removing it must leave recipe 10 with order 1, not with its stored 3.
    [Fact]
    public void RemoveMeal_RenumbersToContiguous_WhenExistingOrdersHaveGaps()
    {
        // Arrange
        var menu = MenuWithOrders((10, 3), (20, 1));

        // Act
        var result = menu.RemoveMeal(20);

        // Assert
        result.IsSuccess.Should().BeTrue();
        AssertLayout(menu, 10);
    }

    // The list holds the meals as [10, 20, 30] but their orders are (3, 1, 2), so the real sequence is 20, 30, 10.
    // After removing 20 two meals remain: renumbering by list position instead of by order would swap them.
    // A menu loaded from the database has no guaranteed list order either, so this is the normal case there.
    [Fact]
    public void RemoveMeal_KeepsTheRemainingMealsInOrder_WhenTheListIsNotSortedByOrder()
    {
        // Arrange
        var menu = MenuWithOrders((10, 3), (20, 1), (30, 2));

        // Act
        var result = menu.RemoveMeal(20);

        // Assert
        result.IsSuccess.Should().BeTrue();
        AssertLayout(menu, 30, 10);
    }

    // Recipe ids differ from positions, so a lookup by position moves the wrong meal.
    // Row (22, 2) is a move to the position the meal already has: a no-op, not an error.
    [Theory]
    [InlineData(11, 3, new[] { 22, 33, 11, 44 })]
    [InlineData(11, 4, new[] { 22, 33, 44, 11 })]
    [InlineData(44, 1, new[] { 44, 11, 22, 33 })]
    [InlineData(44, 2, new[] { 11, 44, 22, 33 })]
    [InlineData(22, 2, new[] { 11, 22, 33, 44 })]
    public void MoveMeal_ProducesExpectedLayout(int recipeId, int newPosition, int[] expectedRecipeIds)
    {
        // Arrange
        var menu = MenuWith(11, 22, 33, 44);

        // Act
        var result = menu.MoveMeal(recipeId, newPosition);

        // Assert
        result.IsSuccess.Should().BeTrue();
        AssertLayout(menu, expectedRecipeIds);
    }

    // With 4 meals the valid positions are 1..4: reordering does not allow n+1, unlike inserting.
    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void MoveMeal_Fails_WhenPositionIsOutOfRange_AndChangesNothing(int newPosition)
    {
        // Arrange
        var menu = MenuWith(11, 22, 33, 44);

        // Act
        var result = menu.MoveMeal(22, newPosition);

        // Assert
        result.Errors.Should().Contain(DomainErrors.Menu.InvalidMealOrder);
        AssertLayout(menu, 11, 22, 33, 44);
    }

    // Recipe id 1 equals the position of the first meal but is not a recipe in this day.
    [Fact]
    public void MoveMeal_Fails_WhenMealIsNotInTheDay_AndChangesNothing()
    {
        // Arrange
        var menu = MenuWith(11, 22);

        // Act
        var result = menu.MoveMeal(1, 2);

        // Assert
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Meal.NotFoundInTheDay(1));
        result.Error.Type.Should().Be(ErrorType.NotFound);
        AssertLayout(menu, 11, 22);
    }

    // The list holds [10, 20, 30] but the orders (3, 1, 2) make the real sequence 20, 30, 10.
    [Theory]
    [InlineData(10, 1, new[] { 10, 20, 30 })]
    [InlineData(20, 3, new[] { 30, 10, 20 })]
    [InlineData(30, 1, new[] { 30, 20, 10 })]
    public void MoveMeal_TreatsOrdersAsRanks_WhenTheListIsNotSortedByOrder(int recipeId, int newPosition, int[] expectedRecipeIds)
    {
        // Arrange
        var menu = MenuWithOrders((10, 3), (20, 1), (30, 2));

        // Act
        var result = menu.MoveMeal(recipeId, newPosition);

        // Assert
        result.IsSuccess.Should().BeTrue();
        AssertLayout(menu, expectedRecipeIds);
    }

    // Orders (3, 1): a gap at 2. Recipe 20 is first, so moving it to position 2 puts it after recipe 10.
    [Fact]
    public void MoveMeal_TreatsOrdersAsRanks_WhenOrdersHaveGaps()
    {
        // Arrange
        var menu = MenuWithOrders((10, 3), (20, 1));

        // Act
        var result = menu.MoveMeal(20, 2);

        // Assert
        result.IsSuccess.Should().BeTrue();
        AssertLayout(menu, 10, 20);
    }

    [Fact]
    public void MoveMeal_KeepsTheServingsOfEveryMeal()
    {
        // Arrange
        var menu = TestMenu.Create(TestDates.Oct(1),
        [
            TestActions.AddMeal(TestRecipes.CreateWithId(11), 1, 4),
            TestActions.AddMeal(TestRecipes.CreateWithId(22), 2, 2)
        ]);

        // Act
        menu.MoveMeal(11, 2);

        // Assert
        menu.Meals.Single(m => m.RecipeId == 11).Servings.Should().Be(4);
        menu.Meals.Single(m => m.RecipeId == 22).Servings.Should().Be(2);
    }

    // Three meals, the one in the middle changes: a mutation touching every meal, the first meal or a lookup by
    // position (ids 11, 22, 33 are not positions) shows up here. The layout assertion proves the order is untouched.
    [Fact]
    public void ChangeServings_UpdatesOnlyTheChosenMeal()
    {
        // Arrange
        var menu = MenuWith(11, 22, 33);

        // Act
        var result = menu.ChangeServings(22, 5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        menu.Meals.Single(m => m.RecipeId == 11).Servings.Should().Be(1);
        menu.Meals.Single(m => m.RecipeId == 22).Servings.Should().Be(5);
        menu.Meals.Single(m => m.RecipeId == 33).Servings.Should().Be(1);
        AssertLayout(menu, 11, 22, 33);
    }

    // One serving is the smallest valid value: a check written as "servings > 1" would reject it.
    // The meal starts with 3 servings so that the change to 1 is visible.
    [Fact]
    public void ChangeServings_Accepts_OneServing()
    {
        // Arrange
        var menu = TestMenu.Create(TestDates.Oct(1), [TestActions.AddMeal(TestRecipes.CreateWithId(11), 1, 3)]);

        // Act
        var result = menu.ChangeServings(11, 1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        menu.Meals.Single().Servings.Should().Be(1);
    }

    // The provider gives 0 and -1; the meal starts with 3 servings and must still have them afterwards.
    [Theory]
    [ClassData(typeof(NonPositiveNumbersTestDataProvider))]
    public void ChangeServings_Fails_WhenServingsAreNotPositive_AndChangesNothing(int servings)
    {
        // Arrange
        var menu = TestMenu.Create(TestDates.Oct(1), [TestActions.AddMeal(TestRecipes.CreateWithId(11), 1, 3)]);

        // Act
        var result = menu.ChangeServings(11, servings);

        // Assert
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Meal.InvalidServings(servings));
        menu.Meals.Single().Servings.Should().Be(3);
    }

    // Recipe id 1 equals the position of the first meal but is not a recipe in this day.
    [Fact]
    public void ChangeServings_Fails_WhenMealIsNotInTheDay_AndChangesNothing()
    {
        // Arrange
        var menu = MenuWith(11, 22);

        // Act
        var result = menu.ChangeServings(1, 5);

        // Assert
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Meal.NotFoundInTheDay(1));
        result.Error.Type.Should().Be(ErrorType.NotFound);
        menu.Meals.Should().OnlyContain(m => m.Servings == 1);
    }

    private static Menu MenuWithOrders(params (int RecipeId, int Order)[] meals) =>
        TestMenu.Create(TestDates.Oct(1),
            [.. meals.Select(m => TestActions.AddMeal(TestRecipes.CreateWithId(m.RecipeId), m.Order, 1))]);

    private static Menu MenuWith(params int[] recipeIds) =>
        MenuWithOrders([.. recipeIds.Select((id, i) => (id, i + 1))]);

    // The recipes are in the expected sequence and the orders are exactly 1..n.
    private static void AssertLayout(Menu menu, params int[] expectedRecipeIds)
    {
        menu.Meals.OrderBy(m => m.Order).Select(m => m.RecipeId).Should().Equal(expectedRecipeIds);
        menu.Meals.Select(m => m.Order).Order().Should().Equal(Enumerable.Range(1, expectedRecipeIds.Length));
    }
}
