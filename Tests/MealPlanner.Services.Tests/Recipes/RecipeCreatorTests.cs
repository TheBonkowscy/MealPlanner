using AwesomeAssertions;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Domain.Recipes;
using MealPlanner.Persistence;
using MealPlanner.Services.Recipes;
using MealPlanner.Services.Shared;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Tests.Shared;
using MealPlanner.Tests.Shared.Factories;
using Microsoft.Extensions.Localization;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests.Recipes;

public class RecipeEditorTests
{
    private readonly Mock<IStringLocalizer<Translations>> _localizer = new();
    private readonly RecipeEditor _sut;

    private static readonly Ingredient PreExistingIngredient = TestIngredients.Create("PreExistingIngredient");
    private readonly List<Ingredient> _ingredients = [PreExistingIngredient];
    private readonly List<Recipe> _recipes = [];
    
    public RecipeEditorTests()
    {
        var ctx = new Mock<MealPlannerDbContext>();
        ctx.Setup(x => x.Recipes).ReturnsDbSet(_recipes);
        ctx.Setup(x => x.Recipes.Add(It.IsAny<Recipe>())).Callback<Recipe>(recipe =>
        {
            RandomId.Set(recipe);
            _recipes.Add(recipe);
        });
        
        ctx.Setup(x => x.Ingredients).ReturnsDbSet(_ingredients);
        
        ctx.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        _sut = new RecipeEditor(ctx.Object, new MeasureUnitMapper(_localizer.Object));
    }
    
    [Fact]
    public async Task Create_Fails_WhenRecipeAlreadyExists()
    {
        // Arrange
        var request = NewRequest();
        await _sut.Create(request, CancellationToken.None);
        
        // Act
        var result = await _sut.Create(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(ServiceErrors.Recipe.AlreadyExists);
    }
    
    [Fact]
    public async Task Create_Fails_WhenIngredientDoesNotExist()
    {
        // Arrange
        var request = NewRequest();
        var missingId = Random.Shared.Next(100, 1000);
        request.Ingredients.Add(new AddIngredientRequest(missingId, 1, nameof(MeasureUnit.Bottle)));
        
        // Act
        var result = await _sut.Create(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeTrue();
        result.Errors.Should().ContainEquivalentOf(ServiceErrors.Ingredient.DoesNotExist(missingId));
    }
    
    [Fact]
    public async Task Create_CreatesRecipe_KeepsSameIngredientWithDifferentUnits_SumsQuantityForSameUnit()
    {
        // Arrange
        var request = NewRequest();
        var ingredientByBottles = request.Ingredients.First(x => x.Id == PreExistingIngredient.Id);
        var ingredientByLitres = new AddIngredientRequest(PreExistingIngredient.Id, 1, nameof(MeasureUnit.Liter));
        request.Ingredients.Add(ingredientByLitres);
        request.Ingredients.Add(ingredientByBottles);
        
        // Act
        var result = await _sut.Create(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeFalse();
        var createdRecipe = _recipes.FirstOrDefault(x => x.Id == result.Value.Id);
        createdRecipe.Should().NotBeNull();
        createdRecipe.Ingredients.Should().HaveCount(2);

        var byBottles = createdRecipe.Ingredients.FirstOrDefault(x => x.Unit == MeasureUnit.Bottle);
        byBottles.Should().NotBeNull();
        byBottles.Quantity.Should().Be(ingredientByBottles.Quantity * 2);
        byBottles.Unit.ToString().Should().Be(ingredientByBottles.Unit);

        var byLitres = createdRecipe.Ingredients.FirstOrDefault(x => x.Unit == MeasureUnit.Liter);
        byLitres.Should().NotBeNull();
        byLitres.Quantity.Should().Be(ingredientByLitres.Quantity);
        byLitres.Unit.ToString().Should().Be(ingredientByLitres.Unit);
    }
    
    // TODO: test for when step mapping results in error
    // TODO: test for when prep step mapping results in error
    
    [Fact]
    public async Task Create_Succeeds()
    {
        // Arrange
        var request = NewRequest();
        
        // Act
        var result = await _sut.Create(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.IsFailure.Should().BeFalse();
        result.Value.Id.Should().BeGreaterThan(0);
    }

    private static CreateRecipeRequest NewRequest()
    {
        var ingredient = new AddIngredientRequest(PreExistingIngredient.Id, 1, nameof(MeasureUnit.Bottle));
        var step = new AddRecipeStepRequest(1, "Step 1");
        var prep = new AddRecipePreparationsRequest("Preparation 1", 1, true);
        return new CreateRecipeRequest(Guid.NewGuid().ToString(), 1, [ingredient], [step], [prep]);
    }
}