using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Shared.Menus.Responses;

namespace MealPlanner.Client;

public interface IUpdateMenus
{
    Task<UpdateMenuResponse> Update(UpdateMenuRequest request, CancellationToken cancellationToken);
}