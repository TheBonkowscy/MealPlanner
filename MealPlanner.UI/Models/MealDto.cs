namespace MealPlanner.UI.Models;

public class MealDto : IReorderable
{
    public string ZoneIdentifier => "DayMenuZone";
    
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public int Servings { get; set; }
}
