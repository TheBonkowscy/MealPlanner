using MealPlanner.Domain.Menus.Actions;
using MealPlanner.Domain.Menus.Exceptions;
using MealPlanner.Domain.Recipes;

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
    
    public void AddMeal(AddMealAction action) => TryAddMeal(action.Order, action.Recipe, action.Servings);

    private void TryAddMeal(int order, Recipe recipe, int servings)
    {
        ValidateOrderAndThrow(order);
        ValidateRecipeAndThrow(recipe);
        var item = Meal.Create(this, recipe, order, servings);
        _meals.Add(item);
    }

    private void ValidateOrderAndThrow(int order)
    {
        InvalidMealOrderException.ThrowIfExceedsNumberOfMeals(order, _meals.Count);

        var mealAtIndex = GetRecipe(order);
        MealExistsAtPositionException.ThrowIfExists(mealAtIndex, order);
    }
    
    public Recipe? GetRecipe(int order) => _meals.FirstOrDefault(x => x.Order == order)?.Recipe;

    private void ValidateRecipeAndThrow(Recipe recipe)
    {
        if (HasRecipe(recipe))
        {
            throw new MealAlreadyPresentInTheDayException(recipe.Name, Date);
        }
    }

    private bool HasRecipe(Recipe recipe) => _meals.Any(x => x.Recipe.Equals(recipe));
    
    public static Menu Create(DateOnly date, List<AddMealAction> mealsToAdd)
    {
        DateOutOfRangeException.ThrowIfNotInRange(date);
        
        var menu = new Menu(date);
        mealsToAdd.ForEach(menu.AddMeal);
        return menu;
    }

    public void RemoveAllItems()
    {
        _meals.Clear();
    }
}