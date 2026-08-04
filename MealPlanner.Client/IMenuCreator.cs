using MealPlanner.Shared.Menus.Requests;
using MealPlanner.Shared.Menus.Responses;

namespace MealPlanner.Client;

public interface IMenuCreator
{
    Task<CreateMenuResponse> CreateMenu(CreateMenuRequest createMenuRequest, CancellationToken cancellationToken);
}