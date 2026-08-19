using AwesomeAssertions;
using MealPlanner.Domain;
using MealPlanner.Domain.Ingredients;
using MealPlanner.Persistence;
using MealPlanner.Services.Recipes;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Tests.Shared;
using MealPlanner.Tests.Shared.Factories;
using Microsoft.Extensions.Localization;
using Moq;
using Moq.EntityFrameworkCore;

namespace MealPlanner.Services.Tests.Recipes;

public class RecipeCreatorTests
{
    private readonly Mock<IStringLocalizer<Translations>> _localizer = new();
    private readonly RecipeCreator _sut;

    private static readonly Ingredient PreExistingIngredient = TestIngredients.Create("PreExistingIngredient");
    private readonly List<Ingredient> _ingredients = [PreExistingIngredient];
    private readonly List<Recipe> _recipes = [];
    
    public RecipeCreatorTests()
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
        _sut = new RecipeCreator(ctx.Object, new MeasureUnitMapper(_localizer.Object));
    }
    
    [Fact]
    public async Task Create_Throws_WhenRecipeAlreadyExists()
    {
        // Arrange
        var request = NewRequest();
        await _sut.Create(request, CancellationToken.None);
        
        // Act
        var result = () => _sut.Create(request, CancellationToken.None);
        
        // Assert
        await result.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Recipe '{request.Name}' already exists");
    }
    
    [Fact]
    public async Task Create_Throws_WhenIngredientDoesNotExist()
    {
        // Arrange
        var request = NewRequest();
        request.Ingredients.Add(new AddIngredientRequest(Random.Shared.Next(100, 1000), 1, nameof(MeasureUnit.Bottle)));
        
        // Act
        var result = () => _sut.Create(request, CancellationToken.None);
        
        // Assert
        await result.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("One or more ingredients not found");
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
        var createdRecipe = _recipes.FirstOrDefault(x => x.Id == result.Id);
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
    
    [Fact]
    public async Task Create_Succeeds()
    {
        // Arrange
        var request = NewRequest();
        
        // Act
        var result = await _sut.Create(request, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
    }

    private static CreateRecipeRequest NewRequest()
    {
        var ingredient = new AddIngredientRequest(PreExistingIngredient.Id, 1, nameof(MeasureUnit.Bottle));
        var step = new AddRecipeStepRequest(1, "Step 1");
        return new CreateRecipeRequest(Guid.NewGuid().ToString(), 1, [ingredient], [step]);
    }
}