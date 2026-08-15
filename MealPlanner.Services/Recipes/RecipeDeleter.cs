using MealPlanner.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MealPlanner.Services.Recipes;

public interface IDeleteRecipe
{
    Task Delete(int id, CancellationToken cancellationToken);
}

public class RecipeDeleter(MealPlannerDbContext ctx) : IDeleteRecipe
{
    public async Task Delete(int id, CancellationToken cancellationToken)
    {
        var existingRecipe = await ctx.Recipes
            .Include(x=> x.Ingredients).ThenInclude(x => x.Ingredient)
            .Include(x => x.Steps).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        
        if (existingRecipe is null) return;
        ctx.Recipes.Remove(existingRecipe);
        await ctx.SaveChangesAsync(cancellationToken);
    }
}