using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace MealPlanner.UI.Models;

public class AppBarAction
{
    public string Icon { get; set; } = string.Empty;
    public string Tooltip { get; set; } = string.Empty;
    public Color Color { get; set; } = Color.Inherit;
    public EventCallback OnClick { get; set; }
}