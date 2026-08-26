namespace MealPlanner.Services.Recipes.Preparations;

public interface IDeleteRecipePreparationsInfo
{
    Task DeletePreparations(int recipeId, int preparationInfoId, CancellationToken cancellationToken);
}

public class RecipePreparationsInfoDeleter : IDeleteRecipePreparationsInfo
{
    public Task DeletePreparations(int recipeId, int preparationInfoId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}