using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.Shared;

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

    public static Result<Meal> Create(Menu? menu, Recipe? recipe, int order, int servings)
    {
        var errors = new List<Error>();
        errors.AddRule(menu is not null, DomainErrors.Menu.IsNull);
        errors.AddRule(recipe is not null, DomainErrors.Recipe.IsNull);
        errors.AddRule(order >= Menu.MinOrder, DomainErrors.Meal.InvalidOrder(order));
        errors.AddRule(servings >= 1, DomainErrors.Meal.InvalidServings(servings));
        return errors.Count != 0 ? Result.Failure<Meal>(errors) : Result.Success(new Meal(menu, recipe, order, servings));
    }
}