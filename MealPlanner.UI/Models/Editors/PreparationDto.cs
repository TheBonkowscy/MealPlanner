namespace MealPlanner.UI.Models.Editors;

public class PreparationDto : IReorderable
{
    public Guid ClientGuid { get; init; } = Guid.NewGuid();
    public string ZoneIdentifier => "PreparationsZone";

    public int Id { get; set; }
    public int LeadDays { get; set; }
    public string Description { get; set; }
    public bool Required { get; set; }
}