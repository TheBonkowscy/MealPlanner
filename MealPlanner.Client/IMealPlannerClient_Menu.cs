using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Shared.Menus.Responses;

namespace MealPlanner.Client;

public interface IMenuClient
{
    Task<CreateMenuResponse> CreateMenu(CreateMenuRequest createMenuRequest, CancellationToken cancellationToken);
    
    Task<GetMenuResponse?> Get(int id, CancellationToken cancellationToken);
    
    Task<GetMenuResponse?> Get(DateTime date, CancellationToken cancellationToken);
    
    Task<GetMenuResponse?> GetToday(CancellationToken cancellationToken);
    
    Task<GetExistingMenusResponse> GetRange(DateTime? from, DateTime? to, CancellationToken cancellationToken);
}