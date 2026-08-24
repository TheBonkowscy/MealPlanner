using MealPlanner.Client.Models;
using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Shared.Menus.Responses;

namespace MealPlanner.Client.Menus;

public interface ICreateMenus
{
    Task<ApiResult<CreateMenuResponse>> CreateMenu(CreateMenuRequest createMenuRequest, CancellationToken cancellationToken);
}