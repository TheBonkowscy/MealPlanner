using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.Shared;

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


    public static Result<AddMealAction> Create(Recipe? recipe, int order, int servings)
    {
        var errors = new List<Error>();
        errors.AddRule(recipe is not null, DomainErrors.Recipe.IsNull);
        errors.AddRule(order > 0, DomainErrors.Meal.InvalidOrder(order));
        errors.AddRule(servings > 0, DomainErrors.Meal.InvalidServings(servings));
        if (errors.Count != 0)
        {
            return Result.Failure<AddMealAction>(errors);
        }

        return Result.Success(new AddMealAction
        {
            Order = order,
            Recipe = recipe!,
            Servings = servings
        });
    }
}