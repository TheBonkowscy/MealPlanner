using MealPlanner.Shared.Menus.Responses;

namespace MealPlanner.Client.Menus;

public interface IFindMenus
{
    Task<GetMenuResponse?> Get(int id, CancellationToken cancellationToken);
    
    Task<GetMenuResponse?> Get(DateTime date, CancellationToken cancellationToken);
    
    Task<GetMenuResponse?> GetToday(CancellationToken cancellationToken);
    
    Task<GetExistingMenusResponse> GetRange(DateOnly? from, DateOnly? to, CancellationToken cancellationToken);
}