namespace MealPlanner.Domain;

public class MenuItem
{
    public int MenuId { get; private set; }
    public Menu Menu { get; private set; }
    
    public int RecipeId { get; private set; }
    public Recipe Recipe { get; private set; }
    
    public int Order { get; private set; }

    private MenuItem()
    {
        // For EF Core
    }
    
    private MenuItem(Menu menu, Recipe recipe, int order)
    {
        Menu = menu;
        MenuId = menu.Id;
        Recipe = recipe;
        RecipeId = recipe.Id;
        Order = order;
    }

    public static MenuItem Create(Menu menu, Recipe recipe, int order)
    {
        ValidateMenuAndThrow(menu);
        ValidateMealAndThrow(recipe);
        ValidateOrderAndThrow(order);
        
        return new MenuItem(menu, recipe, order);
    }
    
    private static void ValidateMenuAndThrow(Menu menu)
    {
        if (menu is null)
        {
            throw new ArgumentNullException(nameof(menu));
        }
    }
    
    private static void ValidateMealAndThrow(Recipe recipe)
    {
        if (recipe is null)
        {
            throw new ArgumentNullException(nameof(recipe));
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