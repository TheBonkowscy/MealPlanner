using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Shared.Menus.Responses;

namespace MealPlanner.Client;

public interface IFindMenus
{
    Task<GetMenuResponse?> Get(int id, CancellationToken cancellationToken);
    
    Task<GetMenuResponse?> Get(DateTime date, CancellationToken cancellationToken);
    
    Task<GetMenuResponse?> GetToday(CancellationToken cancellationToken);
    
    Task<GetExistingMenusResponse> GetRange(DateOnly? from, DateOnly? to, CancellationToken cancellationToken);
}