using MealPlanner.Persistence;
using MealPlanner.Shared.Recipes.Requests;

namespace MealPlanner.Services.Recipes;

public interface ICreateRecipe
{
    Task<CreateRecipeResponse> Create(CreateRecipeRequest request, CancellationToken cancellationToken);
}

public class RecipeCreator(MealPlannerDbContext ctx) : ICreateRecipe
{
    public Task<CreateRecipeResponse> Create(CreateRecipeRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}