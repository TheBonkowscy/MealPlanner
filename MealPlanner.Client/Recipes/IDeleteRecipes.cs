namespace MealPlanner.Client.Recipes;

public interface IDeleteRecipes
{
    Task<bool> Delete(int id, CancellationToken cancellationToken);
}