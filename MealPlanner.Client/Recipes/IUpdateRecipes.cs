using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;

namespace MealPlanner.Client.Recipes;

public interface IUpdateRecipes
{
    Task<GetRecipeDetailsResponse> UpdateRecipe(int id, UpdateRecipeRequest updateRecipeRequest, CancellationToken cancellationToken);
    
    Task<GetRecipeDetailsResponse> AddIngredientToRecipe(int id, UpdateRecipeIngredientRequest request, CancellationToken cancellationToken);
    
    Task DeleteIngredientFromRecipe(int id, DeleteRecipeIngredientRequest deleteRecipeIngredientRequest, CancellationToken cancellationToken);
    
    Task<GetRecipeDetailsResponse> AddStep(int id, AddRecipeStepRequest request, CancellationToken cancellationToken);
    
    Task<GetRecipeDetailsResponse> UpdateStep(int id, UpdateRecipeStepRequest request, CancellationToken cancellationToken);
    
    Task DeleteStep(int id, int stepId, CancellationToken cancellationToken);
}