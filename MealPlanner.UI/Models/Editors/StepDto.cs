namespace MealPlanner.UI.Models.Editors;

public class StepDto : IReorderable
{
    public string ZoneIdentifier => "StepsZone";

    public int Order { get; set; }
    public string Instructions { get; set; }
}