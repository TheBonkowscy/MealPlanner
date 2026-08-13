namespace MealPlanner.UI.Models;

public class MealDto : IReorderable
{
    public string ZoneIdentifier => "DayMenuZone";
    
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
}
