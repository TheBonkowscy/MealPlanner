namespace MealPlanner.Domain;

public class MenuItem
{
    public int MenuId { get; private set; }
    public Menu Menu { get; private set; }
    
    public int MealId { get; private set; }
    public Meal Meal { get; private set; }
    
    public int Order { get; private set; }

    private MenuItem()
    {
        // For EF Core
    }
    
    private MenuItem(Menu menu, Meal meal, int order)
    {
        Menu = menu;
        MenuId = menu.Id;
        Meal = meal;
        MealId = meal.Id;
        Order = order;
    }

    public static MenuItem Create(Menu menu, Meal meal, int order)
    {
        ValidateMenuAndThrow(menu);
        ValidateMealAndThrow(meal);
        ValidateOrderAndThrow(order);
        
        return new MenuItem(menu, meal, order);
    }
    
    private static void ValidateMenuAndThrow(Menu menu)
    {
        if (menu is null)
        {
            throw new ArgumentNullException(nameof(menu));
        }
    }
    
    private static void ValidateMealAndThrow(Meal meal)
    {
        if (meal is null)
        {
            throw new ArgumentNullException(nameof(meal));
        }
    }
    
    private static void ValidateOrderAndThrow(int order)
    {
        if (order < 0)
        {
            throw new ArgumentOutOfRangeException(null, "Order must be a positive number.");
        }
    }   
}