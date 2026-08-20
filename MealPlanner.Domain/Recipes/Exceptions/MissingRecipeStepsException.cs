namespace MealPlanner.Domain.Recipes.Exceptions;

public class MissingRecipeStepsException : Exception
{
    public static void ThrowIfRecipeStepsMissing(ICollection<RecipeStep> recipeSteps)
    {
        if (recipeSteps.Count == 0)
        {
            Throw();
        }
    }
    
    private static void Throw() => throw new MissingRecipeStepsException();
}