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
        ValidateRecipeAndThrow(recipe);
        ValidateOrderAndThrow(order);
        ValidateServingsAndThrow(servings);

        return new AddMealAction()
        {
            Order = order,
            Recipe = recipe,
            Servings = servings
        };
    }

    private static void ValidateRecipeAndThrow(Recipe recipe)
    {
        if (recipe is null)
        {
            throw new ArgumentNullException(null, "Recipe must not be null.");
        }
    }

    private static void ValidateOrderAndThrow(int order)
    {
        if (order < Menu.MinOrder)
        {
            throw new ArgumentOutOfRangeException(null, "Order must be positive.");
        }
    }

    private static void ValidateServingsAndThrow(int servings)
    {
        if (servings <= 0)
        {
            throw new ArgumentOutOfRangeException(null, "Number of servings must be positive.");
        }
    }
}