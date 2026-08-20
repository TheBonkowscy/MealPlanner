namespace MealPlanner.Domain.Recipes.Exceptions;

public class NonUniqueOrderDetectedException : Exception
{
    public static void ThrowIfOrderIsNotUnique(ICollection<RecipeStep> recipeSteps)
    {
        var uniqueOrdersCount = recipeSteps.Select(x => x.Order).Distinct().Count();
        if (uniqueOrdersCount != recipeSteps.Count)
        {
            Throw();
        }
    }
    
    private static void Throw() => throw new NonUniqueOrderDetectedException();
}