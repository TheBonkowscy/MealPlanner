using ReadOnlyMealsCollection = System.Collections.Generic.IReadOnlyDictionary<int, MealPlanner.Domain.Meal>;
using MealsCollection = System.Collections.Concurrent.ConcurrentDictionary<int, MealPlanner.Domain.Meal>;

namespace MealPlanner.Domain;

public class DailyMenu
{
    public static readonly DateOnly MinDateInThePast = new(2019, 9, 28);

    private MealsCollection _meals = new();

    public Guid Id { get; } = Guid.NewGuid();
    public DateOnly Date { get; private set; }
    public ReadOnlyMealsCollection Meals
    {
        get => _meals;
        private set => value.ToList().ForEach(TryAddMeal);
    }

    private DailyMenu(DateOnly date, MealsCollection meals)
    {
        Date = date;
        Meals = meals;
    }

    public void AddMeal(Meal meal) => AddMeal(Meals.Keys.Count(), meal);

    public void AddMeal(int order, Meal meal) => TryAddMeal(order, meal);

    private void TryAddMeal(KeyValuePair<int, Meal> mealAndOrder)
    {
        mealAndOrder.Deconstruct(out var order, out var meal);
        TryAddMeal(order, meal);
    }

    private void TryAddMeal(int order, Meal meal)
    {
        ValidateOrderAndThrow(order);
        ValidateMealAndThrow(meal);
        if (!_meals.TryAdd(order, meal))
        {
            throw new InvalidOperationException($"Could not add meal to daily menu due to an error.");
        }
    }

    private void ValidateOrderAndThrow(int order)
    {
        if (order < 0)
        {
            throw new ArgumentOutOfRangeException(null, "Order must be a positive number.");
        }

        if (order > _meals.Keys.Count)
        {
            throw new ArgumentOutOfRangeException(null, "Order must not exceed the number of already added meals.");
        }

        var mealAtIndex = _meals!.GetValueOrDefault(order, null);
        if (mealAtIndex is not null)
        {
            throw new InvalidOperationException($"There is already a meal added as #{order + 1} in the day");
        }
    }

    private void ValidateMealAndThrow(Meal meal)
    {
        if (_meals.Values.Any(meal.Equals))
        {
            throw new InvalidOperationException($"Meal '{meal}' is already present in the daily menu for {Date}.");
        }
    }
    
    public static DailyMenu Create(DateOnly date)
    {
        ValidateDateAndThrow(date);
        return new DailyMenu(date, new MealsCollection());
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