namespace MealPlanner.UI.Models.Editors;

public class StepDto : IReorderable
{
    public Guid ClientGuid { get; init; } = Guid.NewGuid();
    public string ZoneIdentifier => "StepsZone";

    public int Id { get; set; }
    public int Order { get; set; }
    public string Instructions { get; set; }
}