namespace MealPlanner.Domain;

public class Meal
{
    public int Id { get; private set; }

    private Meal()
    {
        // For EF Core
    }
    
    private Meal(string name)
    {
        Name = name;
    }

    public string Name { get; private set; }

    public static Meal Create(string name)
    {
        ValidateNameAndThrow(name);

        return new Meal(name);
    }

    private static void ValidateNameAndThrow(string meal)
    {
        if (string.IsNullOrWhiteSpace(meal))
        {
            throw new ArgumentNullException(null, "Please specify a name of the meal");
        }
    }
}