namespace MealPlanner.Domain;

public class Menu
{
    public static readonly DateOnly MinDateInThePast = new(2019, 9, 28);

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
    
    public void AddMeal(Recipe recipe) => TryAddMeal(_meals.Count, recipe);

    public void AddMeal(int order, Recipe recipe) => TryAddMeal(order, recipe);

    private void TryAddMeal(int order, Recipe recipe)
    {
        ValidateOrderAndThrow(order);
        ValidateRecipeAndThrow(recipe);
        var item = Meal.Create(this, recipe, order);
        _meals.Add(item);
    }

    private void ValidateOrderAndThrow(int order)
    {
        if (order > _meals.Count)
        {
            throw new ArgumentOutOfRangeException(null, "Order must not exceed the number of already added meals.");
        }

        var mealAtIndex = GetRecipe(order);
        if (mealAtIndex is not null)
        {
            throw new InvalidOperationException($"There is already a meal added as #{order + 1} in the day");
        }
    }
    
    public Recipe? GetRecipe(int order) => _meals.FirstOrDefault(x => x.Order == order)?.Recipe;

    private void ValidateRecipeAndThrow(Recipe recipe)
    {
        if (HasRecipe(recipe))
        {
            throw new InvalidOperationException($"Meal '{recipe}' is already present in the menu for {Date}.");
        }
    }

    private bool HasRecipe(Recipe recipe) => _meals.Any(x => x.Recipe.Equals(recipe));
    
    public static Menu Create(DateOnly date, List<Recipe> recipes)
    {
        ValidateDateAndThrow(date);
        var menu = new Menu(date);
        recipes.ForEach(menu.AddMeal);
        return menu;
    }

    private static void ValidateDateAndThrow(DateOnly date)
    {
        DateOnly[] invalidDates = [DateOnly.MinValue, DateOnly.MaxValue];
        var dateTooFarInThePast = date < MinDateInThePast;
        var dateTooFarInTheFuture = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(100) < date;
        var dateIsInvalid = invalidDates.Contains(date) || dateTooFarInThePast || dateTooFarInTheFuture;
        if (dateIsInvalid)
        {
            throw new ArgumentOutOfRangeException(null, $"Invalid date specified. The date can not be before {MinDateInThePast} and must be in the near future.");
        }
    }

    public void RemoveAllItems()
    {
        _meals.Clear();
    }

    public void AddRecipes(List<Recipe> recipes)
    {
        recipes.ForEach(AddMeal);
    }
}