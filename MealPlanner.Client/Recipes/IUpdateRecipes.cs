using MealPlanner.Client.Models;
using MealPlanner.Shared.Recipes.Requests;
using MealPlanner.Shared.Recipes.Responses;

namespace MealPlanner.Client.Recipes;

public interface IUpdateRecipes
{
    Task<ApiResult<GetRecipeDetailsResponse>> UpdateRecipe(int id, UpdateRecipeRequest updateRecipeRequest, CancellationToken cancellationToken);
    
    Task<ApiResult<GetRecipeDetailsResponse>> AddIngredientToRecipe(int id, UpdateRecipeIngredientRequest request, CancellationToken cancellationToken);
    
    Task DeleteIngredientFromRecipe(int id, DeleteRecipeIngredientRequest deleteRecipeIngredientRequest, CancellationToken cancellationToken);
    
    Task<ApiResult<GetRecipeDetailsResponse>> AddStep(int id, AddRecipeStepRequest request, CancellationToken cancellationToken);
    
    Task<ApiResult<GetRecipeDetailsResponse>> UpdateStep(int id, UpdateRecipeStepRequest request, CancellationToken cancellationToken);
    
    Task DeleteStep(int id, int stepId, CancellationToken cancellationToken);
    
    Task<ApiResult<GetRecipeDetailsResponse>> AddPreparations(int id, AddRecipePreparationsRequest request, CancellationToken cancellationToken);
    
    Task<ApiResult<GetRecipeDetailsResponse>> UpdatePreparations(int id, UpdateRecipePreparationsRequest request, CancellationToken cancellationToken);
    
    Task DeletePreparations(int id, int preparationId, CancellationToken cancellationToken);
}