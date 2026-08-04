namespace MealPlanner.Shared.Menus.Responses;

public record GetExistingMenusResponse(IEnumerable<ExistingMenuListItem> ExistingMenus)
{
    public static GetExistingMenusResponse Empty => new([]);
}