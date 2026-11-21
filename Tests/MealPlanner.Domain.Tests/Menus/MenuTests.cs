using AwesomeAssertions;
using MealPlanner.Domain.Menus;
using MealPlanner.Domain.Menus.Actions;
using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.Shared;
using MealPlanner.Tests.Shared.Factories;
using MealPlanner.Tests.Shared.Helpers;

namespace MealPlanner.Domain.Tests.Menus;

public class MenuTests
{
    private static readonly DateOnly SharedDate = DateOnly.FromDateTime(DateTime.UtcNow);
    private static readonly Recipe SharedFirstRecipe = TestRecipes.Create("Fish and chips");
    private static readonly Recipe SharedSecondRecipe = TestRecipes.Create("Pierogi");

    [Theory]
    [ClassData(typeof(InvalidDatesTestDataProvider))]
    public void Create_Fails_WhenDateIsInvalid(DateOnly invalidDate, Error underlyingCause)
    {
        // Arrange
        List<AddMealAction> mealsToAdd = [AddMealAction.Create(SharedFirstRecipe, 1, 1).Value, AddMealAction.Create(SharedSecondRecipe, 2, 1).Value];
            
        // Act
        var result = Menu.Create(invalidDate, mealsToAdd);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(underlyingCause);
    }

    [Theory]
    [MemberData(nameof(ValidDatesSource))]
    public void Create_CreatesSuccessfully(DateOnly validDate)
    {
        // Arrange
        List<AddMealAction> mealsToAdd = [AddMealAction.Create(SharedFirstRecipe, 1, 1).Value, AddMealAction.Create(SharedSecondRecipe, 2, 1).Value];
        
        // Act
        var result = Menu.Create(validDate, mealsToAdd);
        
        // Assert
        result.Value.Date.Should().Be(validDate);
    }

    [Fact]
    public void AddMeal_SuccessfullyAddsMeal_KeepsOrder()
    {
        // Arrange
        List<AddMealAction> mealsToAdd = [AddMealAction.Create(SharedFirstRecipe, 1, 1).Value];
        var menu = Menu.Create(SharedDate, mealsToAdd).Value;
        
        // Act
        var result = menu.AddMeal(AddMealAction.Create(SharedSecondRecipe, 2, 1).Value);
        
        // Assert
        result.IsSuccess.Should().BeTrue();
        menu.Meals.Should().HaveCount(2);
        menu.GetRecipe(1).Should().Be(SharedFirstRecipe);
        menu.GetRecipe(2).Should().Be(SharedSecondRecipe);
    }

    [Fact]
    public void AddMeal_Fails_WhenOrderAlreadyTaken()
    {
        // Arrange
        const int order = 1;
        List<AddMealAction> mealsToAdd = [AddMealAction.Create(SharedFirstRecipe, order, 1).Value];
        var menu = Menu.Create(SharedDate, mealsToAdd).Value;
        
        // Act
        var result = menu.AddMeal(AddMealAction.Create(SharedSecondRecipe, order, 1).Value);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Meal.AlreadyExistsAtPosition(order));
    }

    [Fact]
    public void AddMeal_WithMealAlreadyAdded_FailsException()
    {
        // Arrange
        var firstMeal = AddMealAction.Create(SharedFirstRecipe, 1, 1).Value;
        List<AddMealAction> mealsToAdd = [firstMeal, AddMealAction.Create(SharedSecondRecipe, 2, 1).Value];
        var menu = Menu.Create(SharedDate, mealsToAdd).Value;
        var thirdMeal = AddMealAction.Create(SharedFirstRecipe, 3, 1).Value;
        
        // Act
        var result = menu.AddMeal(thirdMeal);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Meal.AlreadyPresentInTheDay(thirdMeal.Recipe.Name));
    }

    [Fact]
    public void AddMeal_Fails_WhenOrderIsOutOfBounds()
    {
        // Arrange
        List<AddMealAction> mealsToAdd = [AddMealAction.Create(SharedFirstRecipe, 1, 1).Value];
        var menu = Menu.Create(SharedDate, mealsToAdd).Value;
        
        // Act
        var result = menu.AddMeal(AddMealAction.Create(SharedSecondRecipe, 999, 1).Value);
        
        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainErrors.Menu.InvalidMealOrder);
    }

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

    [Fact]
    public void InsertMeal_IntoEmptyMenu_Fails_WhenPositionIsAboveOne()
    {
        // Arrange
        var menu = TestMenu.Create(TestDates.Oct(1), []);

        // Act
        var result = menu.InsertMeal(TestActions.AddMeal(TestRecipes.CreateWithId(1), 2, 1));

        // Assert
        result.IsFailure.Should().BeTrue();
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
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainErrors.Menu.InvalidMealOrder);
        AssertLayout(menu, 1, 2);
    }

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
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Meal.AlreadyPresentInTheDay(existing.Name));
        AssertLayout(menu, 1, 2);
    }

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

    [Fact]
    public void RemoveMeal_Fails_WhenMealIsNotInTheDay_AndChangesNothing()
    {
        // Arrange
        var menu = MenuWith(11, 22);

        // Act
        var result = menu.RemoveMeal(1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Meal.NotFoundInTheDay(1));
        result.Error.Type.Should().Be(ErrorType.NotFound);
        AssertLayout(menu, 11, 22);
    }

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
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().Contain(DomainErrors.Menu.InvalidMealOrder);
        AssertLayout(menu, 11, 22, 33, 44);
    }

    [Fact]
    public void MoveMeal_Fails_WhenMealIsNotInTheDay_AndChangesNothing()
    {
        // Arrange
        var menu = MenuWith(11, 22);

        // Act
        var result = menu.MoveMeal(1, 2);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Meal.NotFoundInTheDay(1));
        result.Error.Type.Should().Be(ErrorType.NotFound);
        AssertLayout(menu, 11, 22);
    }

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

    [Theory]
    [ClassData(typeof(NonPositiveNumbersTestDataProvider))]
    public void ChangeServings_Fails_WhenServingsAreNotPositive_AndChangesNothing(int servings)
    {
        // Arrange
        var menu = TestMenu.Create(TestDates.Oct(1), [TestActions.AddMeal(TestRecipes.CreateWithId(11), 1, 3)]);

        // Act
        var result = menu.ChangeServings(11, servings);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Meal.InvalidServings(servings));
        menu.Meals.Single().Servings.Should().Be(3);
    }

    [Fact]
    public void ChangeServings_Fails_WhenMealIsNotInTheDay_AndChangesNothing()
    {
        // Arrange
        var menu = MenuWith(11, 22);

        // Act
        var result = menu.ChangeServings(1, 5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(DomainErrors.Meal.NotFoundInTheDay(1));
        result.Error.Type.Should().Be(ErrorType.NotFound);
        menu.Meals.Should().OnlyContain(m => m.Servings == 1);
    }

    public static TheoryData<DateOnly> ValidDatesSource
    {
        get
        {
            var data = new TheoryData<DateOnly>
            {
                DateOnly.FromDateTime(DateTime.UtcNow),
                Menu.MinDateInThePast,
                DateOnly.FromDateTime(DateTime.UtcNow).AddYears(100)
                
            };
            return data;
        }
    }

    private static Menu MenuWithOrders(params (int RecipeId, int Order)[] meals) =>
        TestMenu.Create(TestDates.Oct(1),
            [.. meals.Select(m => TestActions.AddMeal(TestRecipes.CreateWithId(m.RecipeId), m.Order, 1))]);

    private static Menu MenuWith(params int[] recipeIds) =>
        MenuWithOrders([.. recipeIds.Select((id, i) => (id, i + 1))]);

    private static void AssertLayout(Menu menu, params int[] expectedRecipeIds)
    {
        menu.Meals.OrderBy(m => m.Order).Select(m => m.RecipeId).Should().Equal(expectedRecipeIds);
        menu.Meals.Select(m => m.Order).Order().Should().Equal(Enumerable.Range(1, expectedRecipeIds.Length));
    }
}