using Microsoft.AspNetCore.Components;

namespace MealPlanner.UI.Services;

public class AppBarAction
{
    public string Icon { get; set; } = string.Empty;
    public string Tooltip { get; set; } = string.Empty;
    public EventCallback OnClick { get; set; }
}

public class AppBarService
{
    private readonly List<AppBarAction> _actions = new();

    // Zdarzenie powiadamiające MainLayout o konieczności przenderowania
    public event Action? OnChange;

    public IReadOnlyList<AppBarAction> Actions => _actions.AsReadOnly();

    public void SetActions(params AppBarAction[] actions)
    {
        _actions.Clear();
        _actions.AddRange(actions);
        NotifyStateChanged();
    }

    public void ClearActions()
    {
        _actions.Clear();
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}