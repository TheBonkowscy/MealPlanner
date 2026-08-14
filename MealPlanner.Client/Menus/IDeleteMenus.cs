namespace MealPlanner.Client.Menus;

public interface IDeleteMenus
{
    Task<bool> Delete(DateOnly date, CancellationToken cancellationToken);
}