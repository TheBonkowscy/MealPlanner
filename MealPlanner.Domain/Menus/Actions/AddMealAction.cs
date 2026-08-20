using MealPlanner.Domain.Menus.Exceptions;
using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.Recipes.Exceptions;

namespace MealPlanner.Domain.Menus.Actions;

public class AddMealAction
{
    public Recipe Recipe { get; private set; }
    public int Order { get; private set; }
    public int Servings { get; private set; }

    private AddMealAction()
    {
        // Prevents creation without validation
    }


    public static AddMealAction Create(Recipe recipe, int order, int servings)
    {
        MissingRecipeException.ThrowIfRecipeIsNull(recipe);
        InvalidMealOrderException.ThrowIfOrderIsInvalid(order);
        InvalidNumberOfMealServingsException.ThrowIfServingsIsInvalid(servings);

        return new AddMealAction()
        {
            Order = order,
            Recipe = recipe,
            Servings = servings
        };
    }
}