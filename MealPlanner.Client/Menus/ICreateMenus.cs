using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Shared.Menus.Responses;

namespace MealPlanner.Client.Menus;

public interface ICreateMenus
{
    Task<CreateMenuResponse> CreateMenu(CreateMenuRequest createMenuRequest, CancellationToken cancellationToken);
}