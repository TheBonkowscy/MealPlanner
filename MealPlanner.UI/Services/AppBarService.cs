using MealPlanner.UI.Models;

namespace MealPlanner.UI.Services;

public class AppBarService
{
    private readonly List<AppBarAction> _actions = [];

    public string Title { get; private set; } = "Meal Planner";
    public IReadOnlyList<AppBarAction> Actions => _actions.AsReadOnly();

    public event Action? OnChange;

    public void SetTitle(string title)
    {
        Title = title;
        NotifyStateChanged();
    }

    public void SetActions(params AppBarAction[] actions)
    {
        _actions.Clear();
        _actions.AddRange(actions);
        NotifyStateChanged();
    }

    public void ClearActions()
    {
        _actions.Clear();
        Title = "Meal Planner";
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}