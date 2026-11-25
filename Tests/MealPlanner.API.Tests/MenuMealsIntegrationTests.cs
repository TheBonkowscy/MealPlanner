using System.Net;
using AwesomeAssertions;
using MealPlanner.API.Tests.Shared;
using MealPlanner.Domain.Menus;
using MealPlanner.Domain.Recipes;
using MealPlanner.Shared.Menus;
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

    private static string BuildGetRoute(DateOnly date) => $"{Constants.MenusRoute}/{date.ToString("O")}";
}