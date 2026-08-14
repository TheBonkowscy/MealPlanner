using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Shared.Menus.Responses;

namespace MealPlanner.Client.Menus;

public interface IUpdateMenus
{
    Task<UpdateMenuResponse> Update(UpdateMenuRequest request, CancellationToken cancellationToken);
}