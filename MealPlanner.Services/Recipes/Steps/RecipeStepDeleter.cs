using MealPlanner.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Recipes.Steps;

public interface IDeleteRecipeStep
{
    Task DeleteStep(int recipeId, int stepId, CancellationToken cancellationToken);
}

public class RecipeStepDeleter(MealPlannerDbContext ctxObject) : IDeleteRecipeStep
{
    public async Task DeleteStep(int recipeId, int stepId, CancellationToken cancellationToken)
    {
        var recipe = await ctxObject.Recipes.Include(x => x.Steps)
            .FirstOrDefaultAsync(x => x.Id == recipeId, cancellationToken: cancellationToken);

        var step = recipe?.Steps.FirstOrDefault(x => x.Id == stepId);
        if (step is null)
        {
            return;
        }
        
        recipe!.RemoveStep(step);
        ctxObject.Recipes.Update(recipe);
        await ctxObject.SaveChangesAsync(cancellationToken);
    }
}