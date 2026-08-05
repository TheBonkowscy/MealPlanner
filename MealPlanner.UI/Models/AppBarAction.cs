using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Models;

public class AppBarAction
{
    public string Icon { get; set; } = string.Empty;
    public string Tooltip { get; set; } = string.Empty;
    public EventCallback OnClick { get; set; }
}