using System.Net;
using AwesomeAssertions;
using MealPlanner.API.Tests.Shared;
using MealPlanner.Domain.Menus;
using MealPlanner.Domain.Recipes;
using MealPlanner.Shared.Menus;
using MealPlanner.Shared.Menus.Meals;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Shared.Menus.Responses;
using MealPlanner.Shared.Recipes.Responses;
using MealPlanner.Tests.Shared.Factories;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace MealPlanner.API.Tests;

[Collection("IntegrationTests")]
public class MenuMealsIntegrationTests(MealPlannerWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private static readonly DateOnly PlannedDay = TestDates.Oct(14);
    private static readonly DateOnly TargetDay = TestDates.Oct(15);
    private const int MissingRecipeId = 999_999;
    
    [Theory]
    [MemberData(nameof(InsertPositionsWithResultingOrder))]
    public async Task PostMeal_InsertsTheMealAtTheGivenPosition_WhenTheDayHasAMenu(int position, string[] expectedOrder)
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        var pasta = await AddRecipe("Pasta");
        var salad = await AddRecipe("Salad");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2), (pasta, 2, 3));

        // Act
        var result = await Client.PostAsJsonAsync(BuildMealsRoute(PlannedDay), new CreateMealRequest(salad.Id, position, 4));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var day = await GetDay(PlannedDay);
        day.Meals.Select(meal => meal.Name).Should().Equal(expectedOrder);
        day.Meals.Select(meal => meal.Order).Should().Equal(1, 2, 3);
        day.Meals.ToDictionary(meal => meal.Name, meal => meal.Servings)
            .Should().BeEquivalentTo(new Dictionary<string, int> { ["Soup"] = 2, ["Pasta"] = 3, ["Salad"] = 4 });
    }

    public static TheoryData<int, string[]> InsertPositionsWithResultingOrder =>
        new()
        {
            { 1, ["Salad", "Soup", "Pasta"] },
            { 2, ["Soup", "Salad", "Pasta"] },
            { 3, ["Soup", "Pasta", "Salad"] }
        };

    [Fact]
    public async Task PostMeal_CreatesTheMenu_WhenTheDayHasNone()
    {
        // Arrange
        var soup = await AddRecipe("Soup");

        // Act
        var result = await Client.PostAsJsonAsync(BuildMealsRoute(PlannedDay), new CreateMealRequest(soup.Id, 1, 3));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var day = await GetDay(PlannedDay);
        day.Date.Should().Be(PlannedDay);
        day.Meals.Should().Equal(new OrderedRecipeListItemResponse(soup.Id, 1, 3, "Soup"));
    }

    [Fact]
    public async Task PostMeal_ReturnsBadRequest_WhenTheRecipeIsAlreadyInTheDay()
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        var pasta = await AddRecipe("Pasta");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2), (pasta, 2, 3));

        // Act
        var result = await Client.PostAsJsonAsync(BuildMealsRoute(PlannedDay), new CreateMealRequest(soup.Id, 3, 5));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var day = await GetDay(PlannedDay);
        day.Meals.Should().Equal(
            new OrderedRecipeListItemResponse(soup.Id, 1, 2, "Soup"),
            new OrderedRecipeListItemResponse(pasta.Id, 2, 3, "Pasta"));
    }

    [Theory]
    [InlineData("en", "The meal Soup")]
    [InlineData("pl", "Posiłek Soup")]
    public async Task PostMeal_ReturnsLocalizedBadRequest_WhenTheRecipeIsAlreadyInTheDay(string language, string expectedTitleStart)
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2));
        var client = Client;
        client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(language);

        // Act
        var result = await client.PostAsJsonAsync(BuildMealsRoute(PlannedDay), new CreateMealRequest(soup.Id, 2, 5));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await result.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem.Title.Should().StartWith(expectedTitleStart);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(4)]
    public async Task PostMeal_ReturnsBadRequest_WhenThePositionIsOutOfRangeForTheDay(int position)
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        var pasta = await AddRecipe("Pasta");
        var salad = await AddRecipe("Salad");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2), (pasta, 2, 3));

        // Act
        var result = await Client.PostAsJsonAsync(BuildMealsRoute(PlannedDay), new CreateMealRequest(salad.Id, position, 4));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var day = await GetDay(PlannedDay);
        day.Meals.Should().Equal(
            new OrderedRecipeListItemResponse(soup.Id, 1, 2, "Soup"),
            new OrderedRecipeListItemResponse(pasta.Id, 2, 3, "Pasta"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(2)]
    public async Task PostMeal_ReturnsBadRequestAndCreatesNoMenu_WhenThePositionIsOutOfRangeForAnEmptyDay(int position)
    {
        // Arrange
        var soup = await AddRecipe("Soup");

        // Act
        var result = await Client.PostAsJsonAsync(BuildMealsRoute(PlannedDay), new CreateMealRequest(soup.Id, position, 2));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var dayResult = await Client.GetAsync(BuildGetRoute(PlannedDay));
        dayResult.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task PostMeal_ReturnsBadRequestAndCreatesNoMenu_WhenTheServingsAreNotPositive(int servings)
    {
        // Arrange
        var soup = await AddRecipe("Soup");

        // Act
        var result = await Client.PostAsJsonAsync(BuildMealsRoute(PlannedDay), new CreateMealRequest(soup.Id, 1, servings));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var dayResult = await Client.GetAsync(BuildGetRoute(PlannedDay));
        dayResult.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostMeal_ReturnsBadRequestAndCreatesNoMenu_WhenTheRecipeDoesNotExist()
    {
        // Act
        var result = await Client.PostAsJsonAsync(BuildMealsRoute(PlannedDay), new CreateMealRequest(MissingRecipeId, 1, 2));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var dayResult = await Client.GetAsync(BuildGetRoute(PlannedDay));
        dayResult.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [MemberData(nameof(DaysOutsideTheSupportedRange))]
    public async Task PostMeal_ReturnsBadRequestAndCreatesNoMenu_WhenTheDayIsOutsideTheSupportedRange(DateOnly day)
    {
        // Arrange
        var soup = await AddRecipe("Soup");

        // Act
        var result = await Client.PostAsJsonAsync(BuildMealsRoute(day), new CreateMealRequest(soup.Id, 1, 2));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var dayResult = await Client.GetAsync(BuildGetRoute(day));
        dayResult.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public static TheoryData<DateOnly> DaysOutsideTheSupportedRange =>
        new()
        {
            Menu.MinDateInThePast.AddDays(-1),
            DateOnly.FromDateTime(DateTime.UtcNow).AddYears(100).AddDays(1)
        };

    [Fact]
    public async Task PutMeal_ChangesTheServings()
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        var pasta = await AddRecipe("Pasta");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2), (pasta, 2, 3));

        // Act
        var result = await Client.PutAsJsonAsync(BuildMealRoute(PlannedDay, soup.Id), new UpdateMealRequest(Servings: 5, Position: null));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var day = await GetDay(PlannedDay);
        day.Meals.Should().Equal(
            new OrderedRecipeListItemResponse(soup.Id, 1, 5, "Soup"),
            new OrderedRecipeListItemResponse(pasta.Id, 2, 3, "Pasta"));
    }

    [Theory]
    [MemberData(nameof(MovesWithResultingOrder))]
    public async Task PutMeal_MovesTheMealToTheGivenPosition(string movedMeal, int position, string[] expectedOrder)
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        var pasta = await AddRecipe("Pasta");
        var salad = await AddRecipe("Salad");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2), (pasta, 2, 3), (salad, 3, 4));
        var moved = new[] { soup, pasta, salad }.Single(recipe => recipe.Name == movedMeal);

        // Act
        var result = await Client.PutAsJsonAsync(BuildMealRoute(PlannedDay, moved.Id), new UpdateMealRequest(Servings: null, Position: position));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var day = await GetDay(PlannedDay);
        day.Meals.Select(meal => meal.Name).Should().Equal(expectedOrder);
        day.Meals.Select(meal => meal.Order).Should().Equal(1, 2, 3);
        day.Meals.ToDictionary(meal => meal.Name, meal => meal.Servings)
            .Should().BeEquivalentTo(new Dictionary<string, int> { ["Soup"] = 2, ["Pasta"] = 3, ["Salad"] = 4 });
    }

    public static TheoryData<string, int, string[]> MovesWithResultingOrder =>
        new()
        {
            { "Soup", 3, ["Pasta", "Salad", "Soup"] },
            { "Salad", 1, ["Salad", "Soup", "Pasta"] },
            { "Pasta", 3, ["Soup", "Salad", "Pasta"] },
            { "Pasta", 2, ["Soup", "Pasta", "Salad"] }
        };

    [Fact]
    public async Task PutMeal_ChangesTheServingsAndThePosition_InOneRequest()
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        var pasta = await AddRecipe("Pasta");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2), (pasta, 2, 3));

        // Act
        var result = await Client.PutAsJsonAsync(BuildMealRoute(PlannedDay, pasta.Id), new UpdateMealRequest(Servings: 6, Position: 1));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var day = await GetDay(PlannedDay);
        day.Meals.Should().Equal(
            new OrderedRecipeListItemResponse(pasta.Id, 1, 6, "Pasta"),
            new OrderedRecipeListItemResponse(soup.Id, 2, 2, "Soup"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(3)]
    public async Task PutMeal_ReturnsBadRequest_WhenThePositionIsOutOfRangeForTheDay(int position)
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        var pasta = await AddRecipe("Pasta");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2), (pasta, 2, 3));

        // Act
        var result = await Client.PutAsJsonAsync(BuildMealRoute(PlannedDay, soup.Id), new UpdateMealRequest(Servings: null, Position: position));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var day = await GetDay(PlannedDay);
        day.Meals.Should().Equal(
            new OrderedRecipeListItemResponse(soup.Id, 1, 2, "Soup"),
            new OrderedRecipeListItemResponse(pasta.Id, 2, 3, "Pasta"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public async Task PutMeal_ReturnsBadRequest_WhenTheServingsAreNotPositive(int servings)
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2));

        // Act
        var result = await Client.PutAsJsonAsync(BuildMealRoute(PlannedDay, soup.Id), new UpdateMealRequest(Servings: servings, Position: null));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var day = await GetDay(PlannedDay);
        day.Meals.Should().Equal(new OrderedRecipeListItemResponse(soup.Id, 1, 2, "Soup"));
    }

    [Fact]
    public async Task PutMeal_ReturnsBadRequest_WhenThereIsNothingToChange()
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2));

        // Act
        var result = await Client.PutAsJsonAsync(BuildMealRoute(PlannedDay, soup.Id), new UpdateMealRequest(Servings: null, Position: null));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var day = await GetDay(PlannedDay);
        day.Meals.Should().Equal(new OrderedRecipeListItemResponse(soup.Id, 1, 2, "Soup"));
    }

    [Theory]
    [InlineData(9, 3)]
    [InlineData(0, 2)]
    public async Task PutMeal_ChangesNothing_WhenOneOfTheChangesIsInvalid(int servings, int position)
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        var pasta = await AddRecipe("Pasta");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2), (pasta, 2, 3));

        // Act
        var result = await Client.PutAsJsonAsync(BuildMealRoute(PlannedDay, soup.Id), new UpdateMealRequest(Servings: servings, Position: position));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var day = await GetDay(PlannedDay);
        day.Meals.Should().Equal(
            new OrderedRecipeListItemResponse(soup.Id, 1, 2, "Soup"),
            new OrderedRecipeListItemResponse(pasta.Id, 2, 3, "Pasta"));
    }

    [Fact]
    public async Task PutMeal_ReturnsNotFound_WhenTheMealIsNotInTheDay()
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        var pasta = await AddRecipe("Pasta");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2));

        // Act
        var result = await Client.PutAsJsonAsync(BuildMealRoute(PlannedDay, pasta.Id), new UpdateMealRequest(Servings: 5, Position: null));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await result.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem.Type.Should().Be("NotFoundInTheDay");
        var day = await GetDay(PlannedDay);
        day.Meals.Should().Equal(new OrderedRecipeListItemResponse(soup.Id, 1, 2, "Soup"));
    }

    [Fact]
    public async Task PutMeal_ReturnsNotFound_WhenTheDayHasNoMenu()
    {
        // Arrange
        var soup = await AddRecipe("Soup");

        // Act
        var result = await Client.PutAsJsonAsync(BuildMealRoute(PlannedDay, soup.Id), new UpdateMealRequest(Servings: 5, Position: null));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await result.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem.Type.Should().Be("MenuDoesNotExist");
    }

    [Fact]
    public async Task DeleteMeal_RemovesTheMealAndClosesTheGap()
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        var pasta = await AddRecipe("Pasta");
        var salad = await AddRecipe("Salad");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2), (pasta, 2, 3), (salad, 3, 4));

        // Act
        var result = await Client.DeleteAsync(BuildMealRoute(PlannedDay, pasta.Id));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var day = await GetDay(PlannedDay);
        day.Meals.Should().Equal(
            new OrderedRecipeListItemResponse(soup.Id, 1, 2, "Soup"),
            new OrderedRecipeListItemResponse(salad.Id, 2, 4, "Salad"));
    }

    [Fact]
    public async Task DeleteMeal_DeletesTheMenu_WhenItWasTheLastMeal()
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2));

        // Act
        var result = await Client.DeleteAsync(BuildMealRoute(PlannedDay, soup.Id));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var dayResult = await Client.GetAsync(BuildGetRoute(PlannedDay));
        dayResult.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteMeal_ReturnsNotFound_WhenTheMealIsNotInTheDay()
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        var pasta = await AddRecipe("Pasta");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2));

        // Act
        var result = await Client.DeleteAsync(BuildMealRoute(PlannedDay, pasta.Id));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await result.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem.Type.Should().Be("NotFoundInTheDay");
        var day = await GetDay(PlannedDay);
        day.Meals.Should().Equal(new OrderedRecipeListItemResponse(soup.Id, 1, 2, "Soup"));
    }

    [Fact]
    public async Task DeleteMeal_ReturnsNotFound_WhenTheDayHasNoMenu()
    {
        // Arrange
        var soup = await AddRecipe("Soup");

        // Act
        var result = await Client.DeleteAsync(BuildMealRoute(PlannedDay, soup.Id));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await result.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem.Type.Should().Be("MenuDoesNotExist");
    }

    [Theory]
    [MemberData(nameof(TransferPositionsWithResultingOrder))]
    public async Task PostTransfer_Move_InsertsTheMealAtTheGivenPositionAndClosesTheGapInTheSourceDay(int position, string[] expectedTargetOrder)
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        var stew = await AddRecipe("Stew");
        var bread = await AddRecipe("Bread");
        var pasta = await AddRecipe("Pasta");
        var salad = await AddRecipe("Salad");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2), (stew, 2, 3), (bread, 3, 4));
        await AddMenuWithMeals(TargetDay, (pasta, 1, 5), (salad, 2, 6));

        // Act
        var result = await Client.PostAsJsonAsync(BuildTransferRoute(PlannedDay, stew.Id), new TransferMealRequest(TargetDay, position, TransferMode.Move));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var source = await GetDay(PlannedDay);
        source.Meals.Should().Equal(
            new OrderedRecipeListItemResponse(soup.Id, 1, 2, "Soup"),
            new OrderedRecipeListItemResponse(bread.Id, 2, 4, "Bread"));
        var target = await GetDay(TargetDay);
        target.Meals.Select(meal => meal.Name).Should().Equal(expectedTargetOrder);
        target.Meals.Select(meal => meal.Order).Should().Equal(1, 2, 3);
        target.Meals.ToDictionary(meal => meal.Name, meal => meal.Servings)
            .Should().BeEquivalentTo(new Dictionary<string, int> { ["Pasta"] = 5, ["Salad"] = 6, ["Stew"] = 3 });
    }

    public static TheoryData<int, string[]> TransferPositionsWithResultingOrder =>
        new()
        {
            { 1, ["Stew", "Pasta", "Salad"] },
            { 2, ["Pasta", "Stew", "Salad"] },
            { 3, ["Pasta", "Salad", "Stew"] }
        };

    [Fact]
    public async Task PostTransfer_Move_CreatesTheTargetMenu_WhenTheTargetDayHasNone()
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        var stew = await AddRecipe("Stew");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2), (stew, 2, 3));

        // Act
        var result = await Client.PostAsJsonAsync(BuildTransferRoute(PlannedDay, stew.Id), new TransferMealRequest(TargetDay, 1, TransferMode.Move));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var source = await GetDay(PlannedDay);
        source.Meals.Should().Equal(new OrderedRecipeListItemResponse(soup.Id, 1, 2, "Soup"));
        var target = await GetDay(TargetDay);
        target.Date.Should().Be(TargetDay);
        target.Meals.Should().Equal(new OrderedRecipeListItemResponse(stew.Id, 1, 3, "Stew"));
    }

    [Fact]
    public async Task PostTransfer_Move_DeletesTheSourceMenu_WhenItWasTheLastMeal()
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        var pasta = await AddRecipe("Pasta");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2));
        await AddMenuWithMeals(TargetDay, (pasta, 1, 5));

        // Act
        var result = await Client.PostAsJsonAsync(BuildTransferRoute(PlannedDay, soup.Id), new TransferMealRequest(TargetDay, 2, TransferMode.Move));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await AssertDayHasNoMenu(PlannedDay);
        var target = await GetDay(TargetDay);
        target.Meals.Should().Equal(
            new OrderedRecipeListItemResponse(pasta.Id, 1, 5, "Pasta"),
            new OrderedRecipeListItemResponse(soup.Id, 2, 2, "Soup"));
    }

    [Fact]
    public async Task PostTransfer_Move_CreatesTheTargetMenuAndDeletesTheSourceMenu_WhenTheLastMealMovesToADayWithNoMenu()
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2));

        // Act
        var result = await Client.PostAsJsonAsync(BuildTransferRoute(PlannedDay, soup.Id), new TransferMealRequest(TargetDay, 1, TransferMode.Move));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        await AssertDayHasNoMenu(PlannedDay);
        var target = await GetDay(TargetDay);
        target.Meals.Should().Equal(new OrderedRecipeListItemResponse(soup.Id, 1, 2, "Soup"));
    }

    [Fact]
    public async Task PostTransfer_Copy_InsertsTheMealWithTheSameServingsAndKeepsTheSourceDay()
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        var stew = await AddRecipe("Stew");
        var pasta = await AddRecipe("Pasta");
        var salad = await AddRecipe("Salad");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2), (stew, 2, 3));
        await AddMenuWithMeals(TargetDay, (pasta, 1, 5), (salad, 2, 6));

        // Act
        var result = await Client.PostAsJsonAsync(BuildTransferRoute(PlannedDay, stew.Id), new TransferMealRequest(TargetDay, 2, TransferMode.Copy));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var source = await GetDay(PlannedDay);
        source.Meals.Should().Equal(
            new OrderedRecipeListItemResponse(soup.Id, 1, 2, "Soup"),
            new OrderedRecipeListItemResponse(stew.Id, 2, 3, "Stew"));
        var target = await GetDay(TargetDay);
        target.Meals.Should().Equal(
            new OrderedRecipeListItemResponse(pasta.Id, 1, 5, "Pasta"),
            new OrderedRecipeListItemResponse(stew.Id, 2, 3, "Stew"),
            new OrderedRecipeListItemResponse(salad.Id, 3, 6, "Salad"));
    }

    [Fact]
    public async Task PostTransfer_Copy_KeepsTheSourceMenuAndCreatesTheTargetMenu_WhenTheLastMealIsCopiedToADayWithNoMenu()
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 3));

        // Act
        var result = await Client.PostAsJsonAsync(BuildTransferRoute(PlannedDay, soup.Id), new TransferMealRequest(TargetDay, 1, TransferMode.Copy));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var source = await GetDay(PlannedDay);
        source.Meals.Should().Equal(new OrderedRecipeListItemResponse(soup.Id, 1, 3, "Soup"));
        var target = await GetDay(TargetDay);
        target.Date.Should().Be(TargetDay);
        target.Meals.Should().Equal(new OrderedRecipeListItemResponse(soup.Id, 1, 3, "Soup"));
    }

    [Theory]
    [InlineData(TransferMode.Move)]
    [InlineData(TransferMode.Copy)]
    public async Task PostTransfer_ReturnsBadRequestAndChangesNothing_WhenTheRecipeIsAlreadyInTheTargetDay(TransferMode mode)
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        var stew = await AddRecipe("Stew");
        var pasta = await AddRecipe("Pasta");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2), (stew, 2, 3));
        await AddMenuWithMeals(TargetDay, (pasta, 1, 5), (stew, 2, 6));

        // Act
        var result = await Client.PostAsJsonAsync(BuildTransferRoute(PlannedDay, stew.Id), new TransferMealRequest(TargetDay, 1, mode));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await result.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem.Type.Should().Be("MealAlreadyPresentInTheDay");
        var source = await GetDay(PlannedDay);
        source.Meals.Should().Equal(
            new OrderedRecipeListItemResponse(soup.Id, 1, 2, "Soup"),
            new OrderedRecipeListItemResponse(stew.Id, 2, 3, "Stew"));
        var target = await GetDay(TargetDay);
        target.Meals.Should().Equal(
            new OrderedRecipeListItemResponse(pasta.Id, 1, 5, "Pasta"),
            new OrderedRecipeListItemResponse(stew.Id, 2, 6, "Stew"));
    }

    [Theory]
    [InlineData(TransferMode.Move)]
    [InlineData(TransferMode.Copy)]
    public async Task PostTransfer_ReturnsBadRequestAndChangesNothing_WhenTheTargetDayIsTheSourceDay(TransferMode mode)
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        var pasta = await AddRecipe("Pasta");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2), (pasta, 2, 3));

        // Act
        var result = await Client.PostAsJsonAsync(BuildTransferRoute(PlannedDay, soup.Id), new TransferMealRequest(PlannedDay, 2, mode));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var day = await GetDay(PlannedDay);
        day.Meals.Should().Equal(
            new OrderedRecipeListItemResponse(soup.Id, 1, 2, "Soup"),
            new OrderedRecipeListItemResponse(pasta.Id, 2, 3, "Pasta"));
    }

    [Theory]
    [InlineData(TransferMode.Move, 0)]
    [InlineData(TransferMode.Move, -1)]
    [InlineData(TransferMode.Move, 4)]
    [InlineData(TransferMode.Copy, 0)]
    [InlineData(TransferMode.Copy, -1)]
    [InlineData(TransferMode.Copy, 4)]
    public async Task PostTransfer_ReturnsBadRequestAndChangesNothing_WhenThePositionIsOutOfRangeForTheTargetDay(TransferMode mode, int position)
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        var stew = await AddRecipe("Stew");
        var pasta = await AddRecipe("Pasta");
        var salad = await AddRecipe("Salad");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2), (stew, 2, 3));
        await AddMenuWithMeals(TargetDay, (pasta, 1, 5), (salad, 2, 6));

        // Act
        var result = await Client.PostAsJsonAsync(BuildTransferRoute(PlannedDay, stew.Id), new TransferMealRequest(TargetDay, position, mode));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var source = await GetDay(PlannedDay);
        source.Meals.Should().Equal(
            new OrderedRecipeListItemResponse(soup.Id, 1, 2, "Soup"),
            new OrderedRecipeListItemResponse(stew.Id, 2, 3, "Stew"));
        var target = await GetDay(TargetDay);
        target.Meals.Should().Equal(
            new OrderedRecipeListItemResponse(pasta.Id, 1, 5, "Pasta"),
            new OrderedRecipeListItemResponse(salad.Id, 2, 6, "Salad"));
    }

    [Theory]
    [InlineData(TransferMode.Move, 0)]
    [InlineData(TransferMode.Move, -1)]
    [InlineData(TransferMode.Move, 2)]
    [InlineData(TransferMode.Copy, 0)]
    [InlineData(TransferMode.Copy, -1)]
    [InlineData(TransferMode.Copy, 2)]
    public async Task PostTransfer_ReturnsBadRequestAndChangesNothing_WhenThePositionIsOutOfRangeForATargetDayWithNoMenu(TransferMode mode, int position)
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2));

        // Act
        var result = await Client.PostAsJsonAsync(BuildTransferRoute(PlannedDay, soup.Id), new TransferMealRequest(TargetDay, position, mode));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var source = await GetDay(PlannedDay);
        source.Meals.Should().Equal(new OrderedRecipeListItemResponse(soup.Id, 1, 2, "Soup"));
        await AssertDayHasNoMenu(TargetDay);
    }

    [Theory]
    [MemberData(nameof(DaysOutsideTheSupportedRange))]
    public async Task PostTransfer_ReturnsBadRequestAndChangesNothing_WhenTheTargetDayIsOutsideTheSupportedRange(DateOnly targetDay)
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2));

        // Act
        var result = await Client.PostAsJsonAsync(BuildTransferRoute(PlannedDay, soup.Id), new TransferMealRequest(targetDay, 1, TransferMode.Move));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var source = await GetDay(PlannedDay);
        source.Meals.Should().Equal(new OrderedRecipeListItemResponse(soup.Id, 1, 2, "Soup"));
        await AssertDayHasNoMenu(targetDay);
    }

    [Fact]
    public async Task PostTransfer_ReturnsNotFound_WhenTheSourceDayHasNoMenu()
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        var pasta = await AddRecipe("Pasta");
        await AddMenuWithMeals(TargetDay, (pasta, 1, 5));

        // Act
        var result = await Client.PostAsJsonAsync(BuildTransferRoute(PlannedDay, soup.Id), new TransferMealRequest(TargetDay, 1, TransferMode.Move));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await result.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem.Type.Should().Be("MenuDoesNotExist");
        var target = await GetDay(TargetDay);
        target.Meals.Should().Equal(new OrderedRecipeListItemResponse(pasta.Id, 1, 5, "Pasta"));
    }

    [Fact]
    public async Task PostTransfer_ReturnsNotFound_WhenTheMealIsNotInTheSourceDay()
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        var pasta = await AddRecipe("Pasta");
        var salad = await AddRecipe("Salad");
        await AddMenuWithMeals(PlannedDay, (soup, 1, 2));
        await AddMenuWithMeals(TargetDay, (salad, 1, 6));

        // Act
        var result = await Client.PostAsJsonAsync(BuildTransferRoute(PlannedDay, pasta.Id), new TransferMealRequest(TargetDay, 1, TransferMode.Move));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await result.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Should().NotBeNull();
        problem.Type.Should().Be("NotFoundInTheDay");
        var source = await GetDay(PlannedDay);
        source.Meals.Should().Equal(new OrderedRecipeListItemResponse(soup.Id, 1, 2, "Soup"));
        var target = await GetDay(TargetDay);
        target.Meals.Should().Equal(new OrderedRecipeListItemResponse(salad.Id, 1, 6, "Salad"));
    }

    private async Task AssertDayHasNoMenu(DateOnly date)
    {
        var result = await Client.GetAsync(BuildGetRoute(date));
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<GetMenuResponse> GetDay(DateOnly date)
    {
        var result = await Client.GetAsync(BuildGetRoute(date));
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<GetMenuResponse>();
        response.Should().NotBeNull();
        return response;
    }

    private async Task<Recipe> AddRecipe(string name)
    {
        var recipe = TestRecipes.Create(name);
        await AddRecipeToDatabase(recipe);
        return recipe;
    }

    private async Task AddMenuWithMeals(DateOnly date, params (Recipe Recipe, int Order, int Servings)[] meals)
    {
        var actions = meals.Select(meal => TestActions.AddMeal(meal.Recipe, meal.Order, meal.Servings)).ToList();
        await AddMenuToDatabase(TestMenu.Create(date, actions));
    }

    private static string BuildMealsRoute(DateOnly date) => $"{Constants.MenusRoute}/{date.ToString("O")}/meals";

    private static string BuildMealRoute(DateOnly date, int recipeId) => $"{BuildMealsRoute(date)}/{recipeId}";

    private static string BuildTransferRoute(DateOnly date, int recipeId) => $"{BuildMealRoute(date, recipeId)}/transfer";

    private static string BuildGetRoute(DateOnly date) => $"{Constants.MenusRoute}/{date.ToString("O")}";
}