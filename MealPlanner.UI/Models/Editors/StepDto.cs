namespace MealPlanner.UI.Models.Editors;

public class StepDto
{
    public int Order { get; set; }
    public string Instructions { get; set; }
    public string ZoneIdentifier { get; } = "StepsZone";
}