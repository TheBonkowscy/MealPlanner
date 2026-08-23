using MealPlanner.Domain.Menus.Actions;
using MealPlanner.Domain.Recipes;
using MealPlanner.Domain.Shared;

namespace MealPlanner.Domain.Menus;

public class Menu
{
    public static readonly DateOnly MinDateInThePast = new(2019, 9, 28);
    public static readonly int MinOrder = 1;

    private List<Meal> _meals = [];

    public int Id { get; private set; }
    public DateOnly Date { get; private set; }
    public IReadOnlyList<Meal> Meals
    {
        get => _meals;
        private set => _meals = [..value];
    }

    private Menu()
    {
        // For EF Core
    }

    private Menu(DateOnly date) : this(date, [])
    {
    }

    private Menu(DateOnly date, List<Meal> meals)
    {
        Date = date;
        Meals = meals;
    }
    
    public Result AddMeal(AddMealAction action) => TryAddMeal(action.Order, action.Recipe, action.Servings);

    private Result TryAddMeal(int order, Recipe recipe, int servings)
    {
        var errors = new List<Error>();
        errors.AddRange(ValidateOrder(order).Errors);
        errors.AddRange(ValidateRecipe(recipe).Errors);
        
        if (errors.Count != 0)
        {
            return Result.Failure(errors);
        }
        
        var meal = Meal.Create(this, recipe, order, servings);
        if (meal.IsFailure)
        {
            return meal;
        }
        _meals.Add(meal.Value);
        return Result.Success();
    }

    private Result ValidateOrder(int order)
    {
        var errors = new List<Error>();
        errors.AddRule(!(order > _meals.Count + 1 && _meals.Count != 0), DomainErrors.Menu.InvalidMealOrder);

        var mealAtIndex = GetRecipe(order);
        errors.AddRule(mealAtIndex is null, DomainErrors.Meal.AlreadyExistsAtPosition);

        return errors.Count != 0 ? Result.Failure(errors) : Result.Success();
    }
    
    public Recipe? GetRecipe(int order) => _meals.FirstOrDefault(x => x.Order == order)?.Recipe;

    private Result ValidateRecipe(Recipe recipe) => HasRecipe(recipe) ? Result.Failure(DomainErrors.Meal.AlreadyPresentInTheDay) : Result.Success();

    private bool HasRecipe(Recipe recipe) => _meals.Any(x => x.Recipe.Equals(recipe));
    
    public static Result<Menu> Create(DateOnly date, List<AddMealAction> mealsToAdd)
    {
        var errors = new List<Error>();
        DateOnly[] invalidDates = [DateOnly.MinValue, DateOnly.MaxValue];
        errors.AddRule(!invalidDates.Contains(date), DomainErrors.Menu.DateIsUnset)
            .AddRule(date >= MinDateInThePast, DomainErrors.Menu.DateTooFarInThePast)
            .AddRule(DateOnly.FromDateTime(DateTime.UtcNow).AddYears(100) >= date, DomainErrors.Menu.DateTooFarInTheFuture);

        if (errors.Count != 0)
        {
            return Result.Failure<Menu>(errors);
        }
        
        var menu = new Menu(date);
        errors.AddRange(mealsToAdd.Select(menu.AddMeal).AllErrors());
        return errors.Count != 0 ? Result.Failure<Menu>(errors) : Result.Success(menu);
    }

    public void RemoveAllItems()
    {
        _meals.Clear();
    }
}