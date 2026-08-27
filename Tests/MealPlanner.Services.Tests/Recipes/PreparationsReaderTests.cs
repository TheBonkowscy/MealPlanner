using AwesomeAssertions;
using MealPlanner.Domain.Menus;
using MealPlanner.Domain.Menus.Actions;
using MealPlanner.Domain.Recipes;
using MealPlanner.Persistence;
using MealPlanner.Services.Recipes.Preparations;
using MealPlanner.Tests.Shared;
using MealPlanner.Tests.Shared.Factories;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests.Recipes;

public class PreparationsReaderTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.Today);

    private readonly PreparationsReader _sut;

    private static readonly List<Menu> _menus = [];
    private static readonly List<Recipe> _recipes = [];

    public PreparationsReaderTests()
    {
        var ctx = new Mock<MealPlannerDbContext>();
        ctx.Setup(x => x.Menus).ReturnsDbSet(_menus);
        ctx.Setup(x => x.Recipes).ReturnsDbSet(_recipes);
        
        _sut = new PreparationsReader(ctx.Object);
    }

    [Fact]
    public async Task Get_ReturnsEmptyList_WhenNoMenusExist()
    {
        // Act
        var result = await _sut.Get(Today, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Preparations.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_ReturnsEmptyList_WhenMealHasNoPrepRequirements()
    {
        // Arrange
        _menus.Clear();
        _recipes.Clear();
        var recipe = TestRecipes.Create(preparations: []);
        CreateAndSaveMenuWithRecipe(Today.AddDays(1), recipe);

        // Act
        var result = await _sut.Get(Today, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Preparations.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_ReturnsPrepItem_WhenPrepMatchesTargetDate()
    {
        // Arrange
        _menus.Clear();
        _recipes.Clear();
        var targetMealDate = Today.AddDays(1);
        const int leadDays = 1;
        var recipe = TestRecipes.Create();
        CreateAndSaveMenuWithRecipe(targetMealDate, recipe);
        var firstPrepRecord = recipe.Preparations[0];

        // Act
        var result = await _sut.Get(Today, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Preparations.Should().HaveCount(1);

        var item = result.Preparations.First();
        item.RecipeId.Should().Be(recipe.Id);
        item.RecipeName.Should().Be(recipe.Name);
        item.MealDate.Should().Be(targetMealDate);
        item.LeadDays.Should().Be(leadDays);
        item.Required.Should().BeFalse();
        item.Description.Should().Be(firstPrepRecord.Description);
    }

    [Fact]
    public async Task Get_IgnoresPrepItems_ThatAreNotDueToday()
    {
        // Arrange
        _menus.Clear();
        _recipes.Clear();
        var recipe = TestRecipes.Create();
        CreateAndSaveMenuWithRecipe(Today.AddDays(3), recipe);

        // Act
        var result = await _sut.Get(Today, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Preparations.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_ReturnsMultipleItems_WhenMultipleRecipesRequirePrepToday()
    {
        // Arrange
        _menus.Clear();
        _recipes.Clear();
        var recipe1 = TestRecipes.Create(preparations: []);
        recipe1.AddPreparations("Initial prep", 1, true);
        CreateAndSaveMenuWithRecipe(Today.AddDays(1), recipe1);

        var recipe2 = TestRecipes.Create(preparations: []);
        recipe2.AddPreparations("Another prep", 2);
        CreateAndSaveMenuWithRecipe(Today.AddDays(2), recipe2);

        // Act
        var result = await _sut.Get(Today, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Preparations.Should().HaveCount(2);
        result.Preparations.Should().Contain(i => i.RecipeName == recipe1.Name && i.Required);
        result.Preparations.Should().Contain(i => i.RecipeName == recipe2.Name && !i.Required);
    }

    private static Menu CreateAndSaveMenuWithRecipe(DateOnly menuDate, Recipe recipe)
    {
        var menu = TestMenu.Create(menuDate, []);
        RandomId.Set(menu);
        RandomId.Set(recipe);

        menu.AddMeal(AddMealAction.Create(recipe, servings: 2, order: 1).Value);
        _menus.Add(menu);
        
        if (_recipes.All(r => r.Id != recipe.Id))
        {
            _recipes.Add(recipe);
        }
        
        return menu;
    }
}