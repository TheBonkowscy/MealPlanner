namespace MealPlanner.Client;

public interface IDeleteMenus
{
    Task<bool> Delete(DateOnly date, CancellationToken cancellationToken);
}