namespace MealPlanner.Domain.Menus;

public class Meal
{
    public int MenuId { get; private set; }
    public Menu Menu { get; private set; }
    
    public int RecipeId { get; private set; }
    public Recipe Recipe { get; private set; }
    
    public int Order { get; private set; }
    public int Servings { get; private set; }

    private Meal()
    {
        // For EF Core
    }
    
    private Meal(Menu menu, Recipe recipe, int order, int servings)
    {
        Menu = menu;
        MenuId = menu.Id;
        Recipe = recipe;
        RecipeId = recipe.Id;
        Order = order;
        Servings = servings;
    }

    public static Meal Create(Menu menu, Recipe recipe, int order, int servings)
    {
        ValidateMenuAndThrow(menu);
        ValidateRecipeAndThrow(recipe);
        ValidateOrderAndThrow(order);
        ValidateServingsAndThrow(servings);
        
        return new Meal(menu, recipe, order, servings);
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

    private static void ValidateServingsAndThrow(int servings)
    {
        if (servings < 0)
        {
            throw new ArgumentOutOfRangeException(null, "Number of servings must be a positive number.");
        }
    }
}