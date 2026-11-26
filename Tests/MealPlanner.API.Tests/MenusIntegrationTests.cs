using System.Net;
using AwesomeAssertions;
using MealPlanner.API.Tests.Shared;
using MealPlanner.Domain.Menus;
using MealPlanner.Domain.Menus.Actions;
using MealPlanner.Domain.Recipes;
using MealPlanner.Shared.Menus;
using MealPlanner.Shared.Menus.Responses;
using MealPlanner.Shared.Recipes.Responses;
using MealPlanner.Tests.Shared.Factories;
using Xunit;

namespace MealPlanner.API.Tests;

[Collection("IntegrationTests")]
public class MenusIntegrationTests(MealPlannerWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);
    private static readonly DateOnly Tomorrow = Today.AddDays(1);
    private static readonly DateOnly SpecificDate = new(2026, 03, 26);
    private static readonly DateOnly WeekStart = TestDates.Oct(12);
    private static readonly Recipe PreExistingRecipe = TestRecipes.Create("Breakfast");
    private static readonly List<AddMealAction> MealsToAdd = [TestActions.AddMeal(PreExistingRecipe, 1, 1)];
    
    [Fact]
    public async Task Get_ById_ReturnsMenuIfExists()
    {
        // Arrange 
        var menu = Menu.Create(Tomorrow, MealsToAdd).Value;
        await AddMenuToDatabase(menu);
        
        // Act
        var result = await Client.GetAsync(BuildGetRoute(menu.Id));
        
        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<GetMenuResponse>();
        response.Should().NotBeNull();
        response.Id.Should().Be(menu.Id);
        response.Date.Should().Be(menu.Date);
    }
    
    [Fact]
    public async Task Get_ById_ReturnsNotFound_WhenMenuDoesNotExist()
    {
        // Act
        var result = await Client.GetAsync(BuildGetRoute(1));
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Get_ForSpecificDate_ReturnsNotFound_WhenMenuDoesNotExist()
    {
        // Act
        var result = await Client.GetAsync(BuildGetRoute(SpecificDate));
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_ForSpecificDate_ReturnsMenuIfExists()
    {
        // Arrange 
        var menu = Menu.Create(SpecificDate, MealsToAdd).Value;
        await AddMenuToDatabase(menu);
        
        // Act
        var result = await Client.GetAsync(BuildGetRoute(menu.Date));
        
        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<GetMenuResponse>();
        response.Should().NotBeNull();
        response.Id.Should().Be(menu.Id);
        response.Date.Should().Be(menu.Date);
    }
    
    [Fact]
    public async Task Get_ForToday_ReturnsNotFound_WhenMenuDoesNotExist()
    {
        // Act
        var result = await Client.GetAsync(BuildGetForTodayRoute());
        
        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Get_ForToday_ReturnsMenuIfExists()
    {
        // Arrange
        var menu = Menu.Create(Today, MealsToAdd).Value;
        await AddMenuToDatabase(menu);
        
        // Act
        var result = await Client.GetAsync(BuildGetForTodayRoute());
        
        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<GetMenuResponse>();
        response.Should().NotBeNull();
        response.Id.Should().Be(menu.Id);
        response.Date.Should().Be(Today);
    }

    [Theory]
    [MemberData(nameof(DateRangeTestData))]
    public async Task Get_ForDateRange(string query, int expectedCount)
    {
        // Arrange
        await AddMenuToDatabase(Menu.Create(Today, MealsToAdd).Value);
        await AddMenuToDatabase(Menu.Create(Tomorrow, MealsToAdd).Value);
        
        // Act
        var result = await Client.GetAsync($"{Constants.MenusRoute}{query}");
        
        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<GetExistingMenusResponse>();
        response.Should().NotBeNull();
        response.ExistingMenus.Should().HaveCount(expectedCount);
    }
    
    public static TheoryData<string, int> DateRangeTestData
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var tomorrow = today.AddDays(1);
            return new TheoryData<string, int>
            {
                { $"?from={today:O}&to={tomorrow:O}", 2 },
                { $"?from={tomorrow.AddDays(1):O}&to={tomorrow.AddDays(2):O}", 0 },
                { $"?from={tomorrow:O}&to={today:O}", 0 },
                { $"?from={tomorrow:O}", 1 },
                { $"?to={today:O}", 1 },
                { $"?from={today:O}&to={today:O}", 1 }
            };
        }
    }

    [Fact]
    public async Task Get_ForDateRange_ReturnsOnlyMenusWithinRange_WhenBothFromAndToProvided()
    {
        // Arrange
        await AddMenuToDatabase(Menu.Create(Today.AddDays(-1), MealsToAdd).Value);
        var menu1 = Menu.Create(Today, MealsToAdd).Value;
        var menu2 = Menu.Create(Tomorrow, MealsToAdd).Value;
        await AddMenuToDatabase(menu1);
        await AddMenuToDatabase(menu2);
        await AddMenuToDatabase(Menu.Create(Tomorrow.AddDays(1), MealsToAdd).Value);

        // Act
        var result = await Client.GetAsync($"{Constants.MenusRoute}?from={Today:O}&to={Tomorrow:O}");

        // Assert
        result.EnsureSuccessStatusCode();
        var response = await result.Content.ReadFromJsonAsync<GetExistingMenusResponse>();
        response.Should().NotBeNull();
        response.ExistingMenus.Should().HaveCount(2);
        response.ExistingMenus.Should().Contain(m => m.Id == menu1.Id);
        response.ExistingMenus.Should().Contain(m => m.Id == menu2.Id);
    }

    [Theory]
    [MemberData(nameof(DeleteMenuData))]
    public async Task Delete_ReturnsNoContent_WhenMenuExists(DateOnly date, Menu? menu)
    {
        // Arrange
        if (menu is not null)
        {
            await AddMenuToDatabase(menu);
        }

        // Act
        var result = await Client.DeleteAsync(BuildEditRoute(date));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    public static IEnumerable<object?[]> DeleteMenuData()
    {
        yield return [DateOnly.FromDateTime(DateTime.Today), null];
        yield return
        [
            DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Menu.Create(Tomorrow, MealsToAdd).Value
        ];
    }

    [Fact]
    public async Task Get_ForWeek_ReturnsSevenDays_WithEmptyMealsForDaysWithoutMenu()
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        var pasta = await AddRecipe("Pasta");
        var salad = await AddRecipe("Salad");
        await AddMenuWithMeals(WeekStart, (soup, 1, 2));
        await AddMenuWithMeals(WeekStart.AddDays(3), (pasta, 1, 4), (salad, 2, 1));
        await AddMenuWithMeals(WeekStart.AddDays(6), (soup, 1, 3));

        // Act
        var result = await Client.GetAsync(BuildGetWeekRoute(WeekStart));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var response = await result.Content.ReadFromJsonAsync<GetWeekResponse>();
        response.Should().NotBeNull();
        response.WeekStart.Should().Be(WeekStart);
        var days = response.Days.ToList();
        days.Select(day => day.Date).Should().Equal(DaysOfWeekStartingOn(WeekStart));
        days[0].Meals.Should().Equal(new OrderedRecipeListItemResponse(soup.Id, 1, 2, "Soup"));
        days[3].Meals.Should().Equal(
            new OrderedRecipeListItemResponse(pasta.Id, 1, 4, "Pasta"),
            new OrderedRecipeListItemResponse(salad.Id, 2, 1, "Salad"));
        days[6].Meals.Should().Equal(new OrderedRecipeListItemResponse(soup.Id, 1, 3, "Soup"));
        new[] { 1, 2, 4, 5 }.Select(index => days[index].Meals).Should().AllSatisfy(meals => meals.Should().BeEmpty());
    }

    [Fact]
    public async Task Get_ForWeek_ReturnsSevenEmptyDays_WhenThereAreNoMenus()
    {
        // Act
        var result = await Client.GetAsync(BuildGetWeekRoute(WeekStart));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var response = await result.Content.ReadFromJsonAsync<GetWeekResponse>();
        response.Should().NotBeNull();
        response.WeekStart.Should().Be(WeekStart);
        response.Days.Select(day => day.Date).Should().Equal(DaysOfWeekStartingOn(WeekStart));
        response.Days.Should().AllSatisfy(day => day.Meals.Should().BeEmpty());
    }

    [Theory]
    [MemberData(nameof(DatesWithTheirWeekStart))]
    public async Task Get_ForWeek_NormalisesTheDateToTheMondayOfItsIsoWeek(DateOnly date, DateOnly expectedWeekStart)
    {
        // Act
        var result = await Client.GetAsync(BuildGetWeekRoute(date));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var response = await result.Content.ReadFromJsonAsync<GetWeekResponse>();
        response.Should().NotBeNull();
        response.WeekStart.Should().Be(expectedWeekStart);
        response.Days.Select(day => day.Date).Should().Equal(DaysOfWeekStartingOn(expectedWeekStart));
    }

    public static TheoryData<DateOnly, DateOnly> DatesWithTheirWeekStart =>
        new()
        {
            { new DateOnly(2026, 10, 12), new DateOnly(2026, 10, 12) },
            { new DateOnly(2026, 10, 14), new DateOnly(2026, 10, 12) },
            { new DateOnly(2026, 10, 18), new DateOnly(2026, 10, 12) },
            { new DateOnly(2026, 12, 31), new DateOnly(2026, 12, 28) },
            { new DateOnly(2027, 1, 1), new DateOnly(2026, 12, 28) },
            { new DateOnly(2027, 1, 3), new DateOnly(2026, 12, 28) },
            { new DateOnly(2025, 12, 31), new DateOnly(2025, 12, 29) },
            { new DateOnly(2026, 1, 1), new DateOnly(2025, 12, 29) }
        };

    [Fact]
    public async Task Get_ForWeek_ReturnsMealsFromBothYears_WhenTheWeekSpansTheYearBoundary()
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        var pasta = await AddRecipe("Pasta");
        var weekStart = new DateOnly(2026, 12, 28);
        await AddMenuWithMeals(new DateOnly(2026, 12, 31), (soup, 1, 2));
        await AddMenuWithMeals(new DateOnly(2027, 1, 1), (pasta, 1, 3));

        // Act
        var result = await Client.GetAsync(BuildGetWeekRoute(new DateOnly(2027, 1, 2)));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var response = await result.Content.ReadFromJsonAsync<GetWeekResponse>();
        response.Should().NotBeNull();
        response.WeekStart.Should().Be(weekStart);
        var days = response.Days.ToList();
        days.Select(day => day.Date).Should().Equal(DaysOfWeekStartingOn(weekStart));
        days[3].Meals.Should().Equal(new OrderedRecipeListItemResponse(soup.Id, 1, 2, "Soup"));
        days[4].Meals.Should().Equal(new OrderedRecipeListItemResponse(pasta.Id, 1, 3, "Pasta"));
        new[] { 0, 1, 2, 5, 6 }.Select(index => days[index].Meals).Should().AllSatisfy(meals => meals.Should().BeEmpty());
    }

    [Fact]
    public async Task Get_ForWeek_ReturnsOnlyMealsFromTheRequestedWeek()
    {
        // Arrange
        var soup = await AddRecipe("Soup");
        var pasta = await AddRecipe("Pasta");
        var salad = await AddRecipe("Salad");
        await AddMenuWithMeals(WeekStart.AddDays(-1), (soup, 1, 1));
        await AddMenuWithMeals(WeekStart, (pasta, 1, 2));
        await AddMenuWithMeals(WeekStart.AddDays(6), (pasta, 1, 2));
        await AddMenuWithMeals(WeekStart.AddDays(7), (salad, 1, 1));

        // Act
        var result = await Client.GetAsync(BuildGetWeekRoute(WeekStart));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var response = await result.Content.ReadFromJsonAsync<GetWeekResponse>();
        response.Should().NotBeNull();
        response.Days.SelectMany(day => day.Meals).Select(meal => meal.Name).Should().Equal("Pasta", "Pasta");
    }

    [Fact]
    public async Task Get_ForWeek_ReturnsMealsOrderedByPosition()
    {
        // Arrange
        var pasta = await AddRecipe("Pasta");
        var soup = await AddRecipe("Soup");
        await AddMenuWithMeals(WeekStart, (pasta, 2, 1), (soup, 1, 1));

        // Act
        var result = await Client.GetAsync(BuildGetWeekRoute(WeekStart));

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        var response = await result.Content.ReadFromJsonAsync<GetWeekResponse>();
        response.Should().NotBeNull();
        response.Days.First().Meals.Select(meal => meal.Name).Should().Equal("Soup", "Pasta");
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

    private static IEnumerable<DateOnly> DaysOfWeekStartingOn(DateOnly monday) => Enumerable.Range(0, 7).Select(monday.AddDays);

    private static string BuildGetWeekRoute(DateOnly date) => $"{Constants.MenuWeeksRoute}/{date.ToString("O")}";

    private static string BuildGetRoute(int id) => $"{Constants.MenusRoute}/{id.ToString()}";

    private static string BuildGetRoute(DateOnly date) => $"{Constants.MenusRoute}/{date.ToString("O")}";

    private static string BuildEditRoute(DateOnly date) => $"{Constants.MenusRoute}/{date.ToString("O")}";
    
    private static string BuildGetForTodayRoute() => $"{Constants.MenusRoute}/today";
}