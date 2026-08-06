namespace MealPlanner.UI.Models;

public class MenuItemDto
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string ZoneIdentifier { get; set; } = "DayMenuZone";
    public int Order { get; set; }
}
