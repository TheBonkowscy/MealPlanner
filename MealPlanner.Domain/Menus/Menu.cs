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
        errors.AddRule(mealAtIndex is null, DomainErrors.Meal.AlreadyExistsAtPosition(order));

        return errors.Count != 0 ? Result.Failure(errors) : Result.Success();
    }
    
    public Recipe? GetRecipe(int order) => _meals.FirstOrDefault(x => x.Order == order)?.Recipe;

    private Result ValidateRecipe(Recipe recipe) => HasRecipe(recipe) ? Result.Failure(DomainErrors.Meal.AlreadyPresentInTheDay(recipe.Name)) : Result.Success();

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

    public Result InsertMeal(AddMealAction addMeal)
    {
        var existingMeal = _meals.FirstOrDefault(x => x.RecipeId == addMeal.Recipe.Id);
        if (existingMeal is not null)
        {
            return Result.Failure(DomainErrors.Meal.AlreadyPresentInTheDay(addMeal.Recipe.Name));
        }

        if (addMeal.Order > _meals.Count + 1)
        {
            return Result.Failure(DomainErrors.Menu.InvalidMealOrder);
        }
        
        var meal = Meal.Create(this, addMeal.Recipe, addMeal.Order, addMeal.Servings);
        if (meal.IsFailure)
        {
            return Result.Failure<Menu>(meal.Errors);
        }
        
        _meals = [.. _meals.OrderBy(x => x.Order)];
        _meals.Insert(addMeal.Order - 1, meal.Value);
        ReindexMeals();
        return Result.Success();
    }

    private void ReindexMeals()
    {
        for (int i = 0; i < _meals.Count; i++)
        {
            _meals[i].UpdateOrder(i + 1);
        }
    }

    public Result RemoveMeal(int recipeId)
    {
        var meal = _meals.FirstOrDefault(x => x.RecipeId == recipeId);
        if (meal is not null)
        {
            _meals = [.. _meals.OrderBy(x => x.Order)];
            _meals.Remove(meal);
            ReindexMeals();
            return Result.Success();
        }

        return Result.Failure(DomainErrors.Meal.NotFoundInTheDay(recipeId));
    }

    public Result MoveMeal(int recipeId, int newOrder)
    {
        var meal =  _meals.FirstOrDefault(x => x.RecipeId == recipeId);
        if (meal is null)
        {
            return Result.Failure(DomainErrors.Meal.NotFoundInTheDay(recipeId));
        }

        if (newOrder <= 0 || newOrder >= _meals.Count + 1)
        {
            return Result.Failure(DomainErrors.Menu.InvalidMealOrder);
        }
        
        _meals = [.. _meals.OrderBy(x => x.Order)];
        _meals.Remove(meal);
        _meals.Insert(newOrder - 1, meal);
        ReindexMeals();
        return Result.Success();
    }

    public Result ChangeServings(int recipeId, int newServings)
    {
        var meal = _meals.FirstOrDefault(x => x.RecipeId == recipeId);
        return meal is null 
            ? Result.Failure(DomainErrors.Meal.NotFoundInTheDay(recipeId)) 
            : meal.UpdateServings(newServings);
    }
}