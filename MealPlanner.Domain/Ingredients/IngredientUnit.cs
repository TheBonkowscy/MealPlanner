namespace MealPlanner.Domain.Ingredients;

public class IngredientUnit
{
    public int Id { get; private set; }
    public string Name { get; private set; }

    private IngredientUnit()
    {
        // For EF Core
    }

    public static IngredientUnit Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(null, "Name cannot be null or whitespace");
        }

        return new IngredientUnit
        {
            Name = name
        };
    }
}