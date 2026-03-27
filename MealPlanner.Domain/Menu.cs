namespace MealPlanner.Domain;

public class Menu
{
    public static readonly DateOnly MinDateInThePast = new(2019, 9, 28);

    private List<MenuItem> _items = [];

    public int Id { get; private set; }
    public DateOnly Date { get; private set; }
    public IReadOnlyList<MenuItem> Items
    {
        get => _items;
        private set => _items = [..value];
    }

    private Menu()
    {
        // For EF Core
    }

    private Menu(DateOnly date, List<MenuItem> items)
    {
        Date = date;
        Items = items;
    }
    
    public void AddMeal(Meal meal) => TryAddMeal(_items.Count, meal);

    public void AddMeal(int order, Meal meal) => TryAddMeal(order, meal);

    private void TryAddMeal(int order, Meal meal)
    {
        ValidateOrderAndThrow(order);
        ValidateMealAndThrow(meal);
        var item = MenuItem.Create(this, meal, order);
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
    
    public Meal? GetMeal(int order) => _items.FirstOrDefault(x => x.Order == order)?.Meal;

    private void ValidateMealAndThrow(Meal meal)
    {
        if (HasMeal(meal))
        {
            throw new InvalidOperationException($"Meal '{meal}' is already present in the menu for {Date}.");
        }
    }

    private bool HasMeal(Meal meal) => _items.Any(x => x.Meal.Equals(meal));
    
    public static Menu Create(DateOnly date)
    {
        ValidateDateAndThrow(date);
        return new Menu(date, []);
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
}