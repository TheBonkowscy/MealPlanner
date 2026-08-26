using MealPlanner.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Recipes.Preparations;

public interface IDeleteRecipePreparations
{
    Task DeletePreparations(int recipeId, int preparationId, CancellationToken cancellationToken);
}

public class RecipePreparationsDeleter(MealPlannerDbContext ctx) : IDeleteRecipePreparations
{
    public async Task DeletePreparations(int recipeId, int preparationId, CancellationToken cancellationToken)
    {
        var recipe = await ctx.Recipes.Include(x => x.Preparations).FirstOrDefaultAsync(x => x.Id == recipeId, cancellationToken);
        
        var prepToRemove = recipe?.Preparations.FirstOrDefault(x => x.Id == preparationId);
        if (prepToRemove is null)
        {
            return;
        }
        
        recipe!.RemovePreparations(prepToRemove);
        ctx.Recipes.Update(recipe);
        await ctx.SaveChangesAsync(cancellationToken);
    }
}