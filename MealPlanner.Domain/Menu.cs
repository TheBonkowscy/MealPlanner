namespace MealPlanner.Domain;

public class Menu
{
    public static readonly DateOnly MinDateInThePast = new(2019, 9, 28);

    private List<Meal> _items = [];

    public int Id { get; private set; }
    public DateOnly Date { get; private set; }
    public IReadOnlyList<Meal> Items
    {
        get => _items;
        private set => _items = [..value];
    }

    private Menu()
    {
        // For EF Core
    }

    private Menu(DateOnly date) : this(date, [])
    {
    }

    private Menu(DateOnly date, List<Meal> items)
    {
        Date = date;
        Items = items;
    }
    
    public void AddMeal(Recipe recipe) => TryAddMeal(_items.Count, recipe);

    public void AddMeal(int order, Recipe recipe) => TryAddMeal(order, recipe);

    private void TryAddMeal(int order, Recipe recipe)
    {
        ValidateOrderAndThrow(order);
        ValidateMealAndThrow(recipe);
        var item = Meal.Create(this, recipe, order);
        _items.Add(item);
    }

    private void ValidateOrderAndThrow(int order)
    {
        if (order > _items.Count)
        {
            throw new ArgumentOutOfRangeException(null, "Order must not exceed the number of already added meals.");
        }

        var mealAtIndex = GetMeal(order);
        if (mealAtIndex is not null)
        {
            throw new InvalidOperationException($"There is already a meal added as #{order + 1} in the day");
        }
    }
    
    public Recipe? GetMeal(int order) => _items.FirstOrDefault(x => x.Order == order)?.Recipe;

    private void ValidateMealAndThrow(Recipe recipe)
    {
        if (HasMeal(recipe))
        {
            throw new InvalidOperationException($"Meal '{recipe}' is already present in the menu for {Date}.");
        }
    }

    private bool HasMeal(Recipe recipe) => _items.Any(x => x.Recipe.Equals(recipe));
    
    public static Menu Create(DateOnly date, List<Recipe> meals)
    {
        ValidateDateAndThrow(date);
        var menu = new Menu(date);
        meals.ForEach(menu.AddMeal);
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
        _items.Clear();
    }

    public void AddMeals(List<Recipe> mappedMeals)
    {
        mappedMeals.ForEach(AddMeal);
    }
}