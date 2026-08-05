namespace MealPlanner.UI.Services;

public interface IMenuStateService
{
    event Action OnMenuChanged;
    void NotifyMenuChanged();
}

public class MenuStateService : IMenuStateService
{
    public event Action? OnMenuChanged;
    
    public void NotifyMenuChanged()
    {
        OnMenuChanged?.Invoke();
    }
}