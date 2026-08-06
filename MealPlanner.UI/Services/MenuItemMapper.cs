using MealPlanner.UI.Models;

namespace MealPlanner.UI.Services;

public class MenuItemMapper
{
    public List<MenuItemDto> MapMenuItem(Dictionary<int, string> menuItems)
    {
        var meals = menuItems.Select(x => new MenuItemDto
        {
            Name = x.Value,
            Order = x.Key,
            ZoneIdentifier = "DayMenuZone"
        }).OrderBy(x => x.Order).ToList();
        return meals;
    }
}