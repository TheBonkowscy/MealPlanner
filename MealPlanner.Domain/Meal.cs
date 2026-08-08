namespace MealPlanner.Domain;

public class Meal
{
    public int MenuId { get; private set; }
    public Menu Menu { get; private set; }
    
    public int RecipeId { get; private set; }
    public Recipe Recipe { get; private set; }
    
    public int Order { get; private set; }

    private Meal()
    {
        // For EF Core
    }
    
    private Meal(Menu menu, Recipe recipe, int order)
    {
        Menu = menu;
        MenuId = menu.Id;
        Recipe = recipe;
        RecipeId = recipe.Id;
        Order = order;
    }

    public static Meal Create(Menu menu, Recipe recipe, int order)
    {
        ValidateMenuAndThrow(menu);
        ValidateRecipeAndThrow(recipe);
        ValidateOrderAndThrow(order);
        
        return new Meal(menu, recipe, order);
    }
    
    private static void ValidateMenuAndThrow(Menu menu)
    {
        if (menu is null)
        {
            throw new ArgumentNullException(nameof(menu));
        }
    }
    
    private static void ValidateRecipeAndThrow(Recipe recipe)
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