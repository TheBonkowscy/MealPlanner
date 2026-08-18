namespace MealPlanner.Services.Recipes.Steps;

public interface IDeleteRecipeStep
{
    Task DeleteStep(int recipeId, int stepId, CancellationToken cancellationToken);
}

public class RecipeStepDeleter : IDeleteRecipeStep
{
    public Task DeleteStep(int recipeId, int stepId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}