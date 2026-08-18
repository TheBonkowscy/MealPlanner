namespace MealPlanner.UI.Models;

public class StepViewItem
{
    public int Order { get; set; }
    public string Instructions { get; set; } = string.Empty;
    public bool IsExpanded { get; set; }
    public bool IsCompleted { get; set; }
}