using MealPlanner.Domain.Menus.Exceptions;
using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.Recipes.Exceptions;

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
        MissingMenuException.ThrowIfMenuIsNull(menu);
        MissingRecipeException.ThrowIfRecipeIsNull(recipe);
        InvalidMealOrderException.ThrowIfOrderIsInvalid(order);
        InvalidNumberOfMealServingsException.ThrowIfServingsIsInvalid(servings);
        
        return new Meal(menu, recipe, order, servings);
    }
}